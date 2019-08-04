package ar100.abe.com.armainapp.dialog;

import android.app.Dialog;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;

import ar100.abe.com.armainapp.R;


public class CustomDialogFragment3 extends CustomDialogBaseFragment {

    EditText mEditX, mEditY, mEditZ;
    CheckBox mCheck;

    @Nullable
    @Override
    public View onCreateView(LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View rootView = inflater.inflate(R.layout.dialog_save, container, false);
        mTag = getTag();
        Button onBtn = (Button)rootView.findViewById(R.id.dia_btn5);
        Button canBtn = (Button)rootView.findViewById(R.id.dia_btn6);
        Button confirmBtn = (Button)rootView.findViewById(R.id.dia_btn7);
        mEditX = (EditText) rootView.findViewById(R.id.save_x);
        mEditY = (EditText) rootView.findViewById(R.id.save_y);
        mEditZ = (EditText) rootView.findViewById(R.id.save_z);
        mCheck = (CheckBox) rootView.findViewById(R.id.check);
        onBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String check = "false";
                if(mCheck.isChecked()){
                    check = "true";
                }
                String text = String.format("%s,%s,%s,%s", mEditX.getText(), mEditY.getText(), mEditZ.getText(), check);
                mCallback.onOkButtonClick(mTag, text);
                Log.v("abeabe", text);
                dismiss();
            }
        });
        canBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                dismiss();
            }
        });
        confirmBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                mCallback.confirmButtonClick();
            }
        });
        return rootView;
    }

    @Override
    public void onActivityCreated(Bundle savedInstanceState) {
        super.onActivityCreated(savedInstanceState);

        Dialog dialog = getDialog();

        dialog.setCancelable(false);

        WindowManager.LayoutParams lp = dialog.getWindow().getAttributes();

        DisplayMetrics metrics = getResources().getDisplayMetrics();
        int dialogWidth = (int) (metrics.widthPixels * 0.8);
        int dialogHeight = (int) (metrics.heightPixels * 0.5);

        lp.width = dialogWidth;
        lp.height = dialogHeight;
        dialog.getWindow().setAttributes(lp);
    }


}

