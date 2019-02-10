using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Common;

public class camera_move : MonoBehaviour {

	// public変数
	public GameObject chara;
	public Image aim;
	public Text obj_name;
	public CameraMode mode;

	// private変数
	Camera cam;
	float cam_theta, cam_phi, cam_radius, cam_height; // カメラ情報
	float cam_theta_dash, cam_phi_dash;
	float cam_rotate_speed;
	float cam_move_speed;
	Vector3 tarpos;
	Vector3 tarpos_previous;

	bool force_track;

	// 初期化
	void Start () {
		cam = GetComponent<Camera>();
		cam_theta = 10.0f * Mathf.Deg2Rad;			// ティルト
		cam_phi = 0.0f;								// トラック
		cam_radius = common.DEFAULT_CAM_RADIUS;		// ドリー
		cam_height = 1.4f;							// 視点の高さ
		mode = CameraMode.THIRD_PERSON;				// カメラモード
		aim.enabled = false;
		cam_rotate_speed = 1.0f;
		cam_move_speed = 4.0f;

		cam_theta_dash = cam_theta;
		cam_phi_dash = cam_phi;

		tarpos = GetTargetPos();
		tarpos_previous = tarpos;
		force_track = false;
	}

	// 更新
	void Update() {
		switch (mode) {
			case CameraMode.FIRST_PERSON:
			if (Input.GetKeyDown(KeyCode.Q)) cam_theta = 0.0f;
			if (Input.GetKeyDown(KeyCode.E)) {
				mode = CameraMode.THIRD_PERSON;
				cam_radius = common.DEFAULT_CAM_RADIUS;
				cam_rotate_speed = 1.0f;
				cam_move_speed = 4.0f;
				cam_theta = 10.0f * Mathf.Deg2Rad;
				chara.transform.rotation = common.QuatRadAxis(cam_phi, Vector3.down);
			}
			float speed;
			speed = Input.GetKey(KeyCode.LeftShift) ? Mathf.PI / 12.0f : Mathf.PI / 2.5f;
			if (Input.GetKey(KeyCode.W)) cam_theta -= speed * Time.deltaTime;
			if (Input.GetKey(KeyCode.S)) cam_theta += speed * Time.deltaTime;
			if (Input.GetKey(KeyCode.A)) cam_phi   += speed * Time.deltaTime;
			if (Input.GetKey(KeyCode.D)) cam_phi   -= speed * Time.deltaTime;
			cam_theta = Mathf.Clamp(cam_theta, -Mathf.PI / 6.0f, Mathf.PI / 6.0f);
			break;

			case CameraMode.THIRD_PERSON:
			if (Input.GetKeyDown(KeyCode.Q)) force_track = true;
			if (Input.GetKeyDown(KeyCode.E)) {
				mode = CameraMode.FIRST_PERSON;
				cam_rotate_speed = 1.0f;
				cam_move_speed = 3.0f;
				cam_theta = 0.0f;
				force_track = true;
			}
			break;
		}
	}
	
	// 更新（Late）
	void LateUpdate () {
		tarpos = GetTargetPos();

		// phiの更新
		if (force_track) force_track = ResetTrack();
		if (mode == CameraMode.THIRD_PERSON) AutomaticTrack(0.75f);
		cam_phi %= 2.0f * Mathf.PI;

		// 回転・移動
		UpdateRotation();
		UpdatePosition();

		// キャラ表示・非表示
		float dist = (transform.position - tarpos).sqrMagnitude;
		if (dist < 0.0625f) {
			cam.cullingMask = ~(1 << 9);
			aim.enabled = true;
		} else {
			cam.cullingMask = ~0;
			aim.enabled = false;
		}

		// previousの更新
		tarpos_previous = tarpos;

		// アウトライン
		OutLine();
	}

	// 注視点の座標取得
	Vector3 GetTargetPos() {
		return chara.transform.position + Vector3.up * cam_height;
	}

	// 回転
	void UpdateRotation() {
		// theta_dash, phi_dashの更新
		float diff = common.RadDiffer(cam_phi, cam_phi_dash);
		float corr = common.QuarticCorr(diff, Mathf.PI, cam_rotate_speed) * Time.deltaTime;
		cam_phi_dash += common.MinAbsf(diff, corr);
		cam_phi_dash %= 2.0f * Mathf.PI;
		if (mode == CameraMode.FIRST_PERSON) cam_radius = common.DEFAULT_CAM_RADIUS / 2.0f * (1.0f - Mathf.Cos(diff)); // カージオイド

		diff = common.RadDiffer(cam_theta, cam_theta_dash);
		corr = common.QuarticCorr(diff, Mathf.PI, cam_rotate_speed) * Time.deltaTime;
		cam_theta_dash += common.MinAbsf(diff, corr);
		cam_theta_dash %= 2.0f * Mathf.PI;

		// 回転の適用
		Quaternion rot;
		rot  = common.QuatRadAxis(cam_phi_dash, Vector3.down); // パン
		rot *= common.QuatRadAxis(cam_theta_dash, Vector3.right); // ティルト
		transform.rotation = rot;
	}

	// 移動
	void UpdatePosition() {
		// 位置の更新
		Vector3 pos;
		Vector3 dir = -transform.forward;
		float dolly = cam_radius;
		RaycastHit hit; // 壁めり込み防止
		Ray ray = new Ray (tarpos, dir);
		int layerMask = ~(1 << 9); // 1111 1111 1111 1111 1111 1101 1111 1111（9番目のレイヤーだけ無視）
		if (Physics.Raycast(ray, out hit, dolly + 0.1f, layerMask)) dolly = (tarpos - (hit.point - dir * 0.1f)).magnitude;
		pos = tarpos + dir * dolly;

		// 位置の適用
		Vector3 deltaP = pos - transform.position;
		float diff = deltaP.magnitude;
		float corr = common.QuarticCorr(diff, 1.0f, cam_move_speed) * Time.deltaTime;
		transform.position += common.MinAbsf(diff, corr) * deltaP.normalized;
	}

	// 自動視点制御
	void AutomaticTrack(float t) {
		Vector3 from = transform.forward;
		Vector3 to = tarpos - tarpos_previous;
		Vector3 normal;
		float deltaA;

		// クランプ
		from.y = 0.0f;
		to.y = 0.0f;

		// 移動していない場合は何もしない
		float norm = to.magnitude;
		if (norm / Time.deltaTime < common.EPSILON) return;

		// 正規化
		from.Normalize();
		to.Normalize();

		// 修正角deltaAを求めphiを更新
		deltaA = Mathf.Atan(norm / cam_radius) * (1.0f - Mathf.Abs(Vector3.Dot(from, to))); // arctan(norm/cam_radius) * (1 - |from・to|)
		normal = Vector3.Cross(from, to);
		if (normal.y > 0.0f) deltaA *= -1.0f;
		cam_phi += deltaA * t;
	}
	
	// 視点リセット
	bool ResetTrack() {
		Vector3 from = Vector3.forward;
		Vector3 to = chara.transform.forward;
		Vector3 normal;
		float theta;

		// ベクトルのなす角thetaを求める
		theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(from, to), -1.0f, 1.0f)); // arccos(from・to)

		// phiを更新
		normal = Vector3.Cross(from, to);
		cam_phi = normal.y > 0.0f ? -theta : theta;
		return false;
	}

	// アウトライン
	void OutLine(){
		RaycastHit hit;
		Ray ray = new Ray (transform.position, transform.forward);
		int layerMask = 1 << 10; // 0000 0000 0000 0000 0000 0100 0000 0000（10番目のレイヤーだけ）
		if (Physics.Raycast(ray, out hit, 3.0f, layerMask)) {
			GameObject obj = hit.collider.gameObject;
			Material mat = obj.GetComponent<Renderer>().material;
			if (mat.HasProperty("_OutlineWidth")) mat.SetFloat("_OutlineWidth", 0.01f);
			obj_name.text = obj.name;
		} else {
			obj_name.text = "";
		}
	}
}
