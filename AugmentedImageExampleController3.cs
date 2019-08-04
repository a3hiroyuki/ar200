using GoogleARCore;
using GoogleARCore.Examples.AugmentedImage;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AugmentedImageExampleController3 : MonoBehaviour
{
    public AugmentedImageVisualizer AugmentedImageVisualizerPrefab;
    public GameObject fitToScanOverlay;
    public GameObject DispPrefab;
    public GameObject ArrowPrefab;

    public GameObject MessageText;
    public GameObject FirstPersonCamera;
    public GameObject PointCloud;
    public GameObject EnemyController;
    public GameObject ARCoreDevice;
    public AudioClip soundDisp;
    public GameObject FitToScanOverlay { get => fitToScanOverlay; set => fitToScanOverlay = value; }

    private GameObject Arrow;
    private GameObject m_disp_obj = null;
    private AudioSource AudioSource;
    private Transform m_cur_transpose = null;
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
    private static Vector3 TREASURE_POSI = new Vector3(-3.0f, 0.5f, -0.5f);
    private bool m_IsDiscovery = false;
    private bool m_IsDisp = false;
    private Dictionary<int, AugmentedImageVisualizer> m_Visualizers = new Dictionary<int, AugmentedImageVisualizer>();
    private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();

    private int DETECTED_PLANE_NUM = 4;
    private int MIN_SEARCH_TIME = 10;
    private int MAX_SEARCH_TIME = 120;

    //追加
    private Pose mTreasure_pose;
    private float mDispTimer;
    private int mDispTimeout;
    private bool m_Is_realmode = false;

    private string ReadText(string filePath, string fileName)
    {
        var combinedPath = Path.Combine(filePath, fileName);
        return File.ReadAllText(combinedPath);
    }

    void Start()
    {
        try
        {
            string str = ReadText(Application.persistentDataPath, "hoge.txt");
            string[] strArr = str.Split(',');
            DETECTED_PLANE_NUM = int.Parse(strArr[0]);
            MAX_SEARCH_TIME = int.Parse(strArr[1]);
            int z = int.Parse(strArr[2]);
            m_Is_realmode = true ? strArr[3].Equals("true") : false;
        }
        catch
        {
            TREASURE_POSI = new Vector3(-5.0f, 1.0f, -0.5f);
        }
        Debug.LogError("abeabe999 " + string.Format("%d %d %d", DETECTED_PLANE_NUM, MAX_SEARCH_TIME));
        mDispTimeout = Random.Range(MIN_SEARCH_TIME, MIN_SEARCH_TIME + 20);
        AudioSource = GetComponent<AudioSource>();
        
    }

    void Update()
    {
        if (m_disp_obj != null)
        {
            if (m_disp_obj.GetComponent<TreasureChestController>().IsGetTreasure())
            {
                MessageText.GetComponent<Text>().text = "おたからゲット！";
            }
            return;
        }

        mDispTimer += Time.deltaTime;

        if (Session.Status == SessionStatus.LostTracking)
        {
            if (mDispTimer > 3)
            {
                Reset();
                MessageText.GetComponent<Text>().text = "まほうがとけました\nスタートへ";
            }
            return;
        }

        if (m_IsDisp)
        {
            Pose p = new Pose(FirstPersonCamera.transform.position, FirstPersonCamera.transform.rotation);
            Vector3 vec = p.position - mTreasure_pose.position;
            Arrow.GetComponent<ArrowController>().Notify2(vec);
            if (IsInRangeForCamera(mTreasure_pose.position))
            {
                //宝発見
                m_disp_obj = Instantiate(DispPrefab, mTreasure_pose.position, mTreasure_pose.rotation);
                MessageText.GetComponent<Text>().text = "カギをさがせ!";
                EnemyController.GetComponent<EnemyARController2>().Notify(m_disp_obj, m_AllPlanes);
                DestroyImmediate(Arrow);
            }
            return;
        }
        
        //宝箱探索開始
        if (m_IsDiscovery)
        {
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
            Debug.LogError("abeabe999 " + m_AllPlanes.Count.ToString());
            if (m_AllPlanes.Count < DETECTED_PLANE_NUM || mDispTimer < mDispTimeout)
            {
                return;
            }
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    int random = Random.Range(0, 4);
                    if (m_IsDisp == false && random < 1)
                    {
                        mTreasure_pose = m_AllPlanes[i].CenterPose;
                        Arrow.GetComponent<ArrowController>().Notify3(false);
                        PointCloud.SetActive(false);
                        m_IsDisp = true;
                        AudioSource.PlayOneShot(soundDisp);
                        return;
                    }
                }
            }
        }

        Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.All);
        foreach (AugmentedImage image in m_TempAugmentedImages)
        {
            AugmentedImageVisualizer visualizer = null;
            m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
            if (image.TrackingState == TrackingState.Tracking && visualizer == null)
            {
                Anchor anchor = image.CreateAnchor(image.CenterPose);
                visualizer = (AugmentedImageVisualizer)Instantiate(AugmentedImageVisualizerPrefab, anchor.transform);
                visualizer.Image = image;
                m_Visualizers.Add(image.DatabaseIndex, visualizer);

                m_cur_transpose = anchor.transform;
                Arrow = Instantiate(ArrowPrefab);
                Arrow.GetComponent<ArrowController>().Notify1(image.CenterPose.position);

                //宝探し開始
                MessageText.GetComponent<Text>().text = "";
                m_IsDiscovery = true;
                PointCloud.SetActive(true);
                Arrow.GetComponent<ArrowController>().Notify3(true);
                mDispTimer = 0;
            }
            else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
            {
                m_Visualizers.Remove(image.DatabaseIndex);
                GameObject.Destroy(visualizer.gameObject);
            }
        }

        // Show the fit-to-scan overlay if there are no images that are Tracking.
        foreach (var visualizer in m_Visualizers.Values)
        {
            if (visualizer.Image.TrackingState == TrackingState.Tracking)
            {
                FitToScanOverlay.SetActive(false);
                return;
            }
        }

        FitToScanOverlay.SetActive(true);
    }

    private void Reset()
    {
        ResetSession();
        mDispTimer = 0;
        foreach (int key in m_Visualizers.Keys)
        {
            AugmentedImageVisualizer v = m_Visualizers[key];
            Destroy(v);
        }
        m_TempAugmentedImages.Clear();
        m_Visualizers.Clear();
       if (Arrow != null)
       {
            DestroyImmediate(Arrow);
       }
        m_cur_transpose = null;
    }

    private void ResetSession()
    {
        ARCoreSession session = ARCoreDevice.GetComponent<ARCoreSession>();
        ARCoreSessionConfig myConfig = session.SessionConfig;
        session.OnDisable();
        session.OnDestroy();
        session.OnEnable();
        session.Awake();
    }

    private bool IsInRangeForCamera(Vector3 plaenPosi)
    {
        //自分自身が向いている方向の単位ベクトル
        float angleDir = FirstPersonCamera.transform.eulerAngles.y * (Mathf.PI / 180.0f);
        Vector3 dir = new Vector3(Mathf.Cos(angleDir) * 0.1f, 0f, Mathf.Sin(angleDir) * 0.1f);

        Vector3 tmpV3 = (plaenPosi - FirstPersonCamera.transform.position).normalized;
        Vector3 tmpV2 = new Vector3(tmpV3.x, 0f, tmpV3.z);//高さyを無視する

        //平面と自分のベクトルの角度差
        float angle = Vector3.Angle(dir, tmpV2);

        float dist = (FirstPersonCamera.transform.position - plaenPosi).sqrMagnitude;

        if (angle < 120 && dist < 0.7)
        {
            return true;
        }
        return false;
    }

    private Pose _WorldToAnchorPose(Pose pose)
    {
        if (m_cur_transpose == null)
        {
            return new Pose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        }
        Matrix4x4 anchorTWorld = Matrix4x4.TRS(m_cur_transpose.position, m_cur_transpose.rotation, Vector3.one).inverse;
        Vector3 position = anchorTWorld.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(anchorTWorld.GetColumn(2), anchorTWorld.GetColumn(1));
        return new Pose(position, rotation);
    }

    private Pose _AnchorPoseToWorld(Pose pose)
    {
        if (m_cur_transpose == null)
        {
            return new Pose(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0));
        }
        Matrix4x4 cloudAnchorMatrix = Matrix4x4.TRS(m_cur_transpose.position, m_cur_transpose.rotation, Vector3.one);
        Vector3 position = cloudAnchorMatrix.MultiplyPoint(pose.position);
        Quaternion rotation = pose.rotation * Quaternion.LookRotation(
            cloudAnchorMatrix.GetColumn(2), cloudAnchorMatrix.GetColumn(1));
        return new Pose(position, rotation);
    }

    

    bool OnTouchDown(GameObject game)
    {
        if (0 < Input.touchCount)
        {
            for (int i = 0; i < Input.touchCount; i++) // タッチされている指の数だけ処理
            {
                // タッチ情報をコピー
                Touch t = Input.GetTouch(i);
                // タッチしたときかどうか
                if (t.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(t.position);
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject == game)
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false; //タッチされてなかったらfalse
    }
}
