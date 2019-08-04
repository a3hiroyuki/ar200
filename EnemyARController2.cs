//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;
using UnityEngine.UI;

public class EnemyARController2 : MonoBehaviour
{
    public Camera FirstPersonCamera;
    public GameObject MonsterPrefab;
    public GameObject BossPrefab;
    public GameObject TreasureChestPrefab;
    public GameObject KeyPrefab;

    private bool m_IsQuitting = false;
    private Vector3 monsterRotation = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 monsterTranslation = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 monsterScale = new Vector3(0.5f, 0.5f, 0.5f);
    private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
    private float mDispTimer = 0f;
    private List<GameObject> mMonsters = new List<GameObject>();
    private Pose mLastDispPose;
    private GameObject mTreasureChest = null;
    private GameObject mKey = null;
    private int mEnemyDispCount = 0;
    public int ENEMY_COUNT = 4;

    private bool m_IsBossDisp;
    public Text scoreText; //Text用変数

    //宝箱を取得
    public void Notify(GameObject treasureChest, List<DetectedPlane> AllPlanes)
    {
        mTreasureChest = treasureChest;
        m_AllPlanes = AllPlanes;
    }

    public void Start()
    {

    }

    public void Update()
    {
        //宝箱を見つけてから敵倒すゲームを開始するまで停止
        if (mTreasureChest == null)
        {
            return;
        }

        //鍵を取得したらゲーム終了
        if (mKey != null)
        {
            return;
        }

        // Hide snackbar when currently tracking at least one plane.
        //Session.GetTrackables<DetectedPlane>(m_AllPlanes, TrackableQueryFilter.All);
        for (int i = 0; i < m_AllPlanes.Count; i++)
        {
            if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
            {
                int random = Random.Range(0, 5);
                if (mDispTimer <= 0 && mEnemyDispCount < ENEMY_COUNT && random < 1)
                {
                    Anchor anchor = m_AllPlanes[i].CreateAnchor(m_AllPlanes[i].CenterPose);
                    GameObject monsterObject = Instantiate(MonsterPrefab, m_AllPlanes[i].CenterPose.position, m_AllPlanes[i].CenterPose.rotation);
                    DispEnemy(monsterObject, anchor);
                    mDispTimer = Random.Range(2.0f, 5.0f);
                    break;

                }
                else if (m_IsBossDisp)
                {
                    m_IsBossDisp = false;
                    mLastDispPose = m_AllPlanes[i].CenterPose;
                    Anchor anchor = m_AllPlanes[i].CreateAnchor(mLastDispPose);
                    GameObject monsterObject = Instantiate(BossPrefab, m_AllPlanes[i].CenterPose.position, m_AllPlanes[i].CenterPose.rotation);
                    DispEnemy(monsterObject, anchor);
                    break;
                }
                
            }
        }

        mDispTimer -= Time.deltaTime;
        for (int i = 0; i < mMonsters.Count; i++)
        {
            GameObject monsterObject = mMonsters[i];
            if (monsterObject.GetComponent<Zonbie2>().is_dead)
            {
                if (monsterObject.name == "Robot Kyle(Clone)")
                {
                    mKey = Instantiate(KeyPrefab, mLastDispPose.position, mLastDispPose.rotation);
                    mKey.transform.position = mLastDispPose.position;
                    mKey.GetComponent<KeyController>().notify(mTreasureChest);
                }
                else
                {
                    Destroy(monsterObject);
                    mMonsters.Remove(monsterObject);
                    if (mEnemyDispCount >= ENEMY_COUNT && mMonsters.Count == 0)
                    {
                        m_IsBossDisp = true;
                    }
                    else
                    {
                        mDispTimer = Random.Range(2.0f, 5.0f);
                    }
                }
            }
        }

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }
    }

    private void DispEnemy(GameObject monsterObject, Anchor anchor)
    {
        monsterObject.transform.Rotate(monsterRotation);
        monsterObject.transform.Translate(monsterTranslation);
        monsterObject.transform.localScale = monsterScale;
        monsterObject.transform.parent = anchor.transform;

        // Let the monster look at the camera but keep it on the ground (plane)
        Vector3 cameraDirection = new Vector3();
        cameraDirection = transform.InverseTransformPoint(FirstPersonCamera.transform.position) - monsterObject.transform.position;
        cameraDirection.y = 0;
        monsterObject.transform.rotation = Quaternion.LookRotation(cameraDirection, monsterObject.transform.TransformVector(Vector3.up));
        mMonsters.Add(monsterObject);
        mEnemyDispCount++;
    }


}
