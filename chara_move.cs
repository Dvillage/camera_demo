using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common;

public class chara_move : MonoBehaviour {

	// public変数

	// private変数
	Camera cam; 			// カメラ
	camera_move cam_mv;
	Animator animator; 		// アニメーション管理
	float velocity;			// キャラの速度

	float wait_time;

	// 初期化
	void Start () {
		cam = Camera.main;
		cam_mv = cam.GetComponent<camera_move>();
		animator = GetComponent<Animator>();
		velocity = 0.0f;

		wait_time = 0.0f;
	}
	
	// 更新
	void Update () {
		Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);
		float speed = 0.0f;
		bool isWalking = Input.GetKey(KeyCode.LeftShift) ? true : false; // ゆっくり歩く

		// キー入力
		if (cam_mv.mode == CameraMode.THIRD_PERSON) {
			if (Input.GetKey(KeyCode.W)) direction +=  cam.transform.forward;
			if (Input.GetKey(KeyCode.S)) direction += -cam.transform.forward;
			if (Input.GetKey(KeyCode.A)) direction += -cam.transform.right;
			if (Input.GetKey(KeyCode.D)) direction +=  cam.transform.right;
			direction.y = 0.0f; // y軸クランプ
		}

		// 動作
		if (direction.sqrMagnitude > common.EPSILON) { // 移動
			Turnaround(direction);
			speed = isWalking ? 0.8f : 2.6f;
			wait_time = 0.0f;
		} else { // 静止
			if (wait_time > 10.0f) { animator.SetTrigger("relax"); wait_time = -10.0f; }
			wait_time += Time.deltaTime;
		}
		VelocityContorol(speed, 5.0f);
		animator.SetFloat("velocity", velocity);
		transform.Translate(0.0f, 0.0f, velocity * Time.deltaTime, Space.Self);
	}

	void VelocityContorol(float speed, float acc) {
		float diff = speed - velocity;
		float corr = common.LinearCorr(diff, acc) * Time.deltaTime;
		velocity += common.MinAbsf(diff, corr);
	}

	/// <summary>
	/// 指定の方向を向く（1/2秒でπ修正）
	/// <param name="to">向かせる方向</param>
	/// </summary>
	void Turnaround (Vector3 to) {
		Vector3 from = transform.forward;
		Vector3 axis;
		float theta;

		// 正規化
		from.Normalize();
		to.Normalize();

		// ベクトルのなす角thetaを求める
		theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(from, to), -1.0f, 1.0f)); // arccos(from・to)
		if (theta < common.EPSILON) return;
		
		// 回転軸と回転角を求め回転
		axis = Vector3.Cross(from, to);
		if (axis.sqrMagnitude < common.EPSILON) axis = Vector3.up; // 外積が0の時は回転軸をy軸にする
		transform.rotation *= common.QuatRadAxis(common.QuarticCorr(theta, Mathf.PI, 2.0f) * Time.deltaTime, axis);
	}
}
