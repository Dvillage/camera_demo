using UnityEngine;
using System.Collections;

// 共通の定数・メソッド
namespace Common {

	/// <summary>
	/// カメラの操作モード
	/// </summary>
	public enum CameraMode {
		FIRST_PERSON,
		THIRD_PERSON
	};

	public static class common {
  		public const float EPSILON = 1.0e-5f; // 微小量
		// public const float RAD_EPSILON = EPSILON * 2.0f * Mathf.PI; // 微小角
		public const float DEFAULT_CAM_RADIUS = 2.8f;
		const float PI = Mathf.PI;
		const float PI_2 = Mathf.PI * 2.0f;

		/// <summary>
		/// 角度（rad）の差を求める
		/// <param name="a">基準角度</param>
		/// <param name="b">比較角度</param>
		/// <returns>return: 差分[-π, +π]</returns>
		/// </summary>
		public static float RadDiffer(float a, float b) {
			float theta = a - b;
			if (theta >  PI) theta -= PI_2;
			if (theta < -PI) theta += PI_2;
			return theta;
		}

		/// <summary>
		/// 回転角と軸からクォータニオンを生成
		/// <param name="theta">角度（rad）</param>
		/// <param name="axis">軸</param>
		/// <returns>return: 生成されたクォータニオン</returns>
		/// </summary>
		public static Quaternion QuatRadAxis(float theta, Vector3 axis) {
			float sin = Mathf.Sin(theta / 2.0f);
			float cos = Mathf.Cos(theta / 2.0f);
			axis.Normalize();
			return new Quaternion(axis.x*sin, axis.y*sin, axis.z*sin, cos);
		}

		/// <summary>
		/// 1次関数f(t) = atによる補間で0に修正する傾き∇f(t)を求める
		/// <param name="ft">修正したい値（目標値-現在）</param>
		/// <param name="a">関数の係数</param>
		/// <returns>return: ∇f(t)</returns>
		/// </summary>
		public static float LinearCorr(float ft, float a) {
			float sign = ft < 0.0f ? -1.0f : 1.0f;
			ft *= sign;
			return sign * a;
		}

		/// <summary>
		/// 4次関数f(t) = a(bt)^4による補間で0に修正する傾き∇f(t)を求める
		/// <param name="ft">修正したい値（目標値-現在）</param>
		/// <param name="a">関数の係数</param>
		/// <param name="b">関数の係数</param>
		/// <returns>return: ∇f(t)</returns>
		/// </summary>
		public static float QuarticCorr(float ft, float a, float b) {
			float sign = ft < 0.0f ? -1.0f : 1.0f;
			float t;
			ft *= sign;
			t = Mathf.Sqrt(Mathf.Sqrt(ft / a));
			return sign * 4.0f * a * b * t * t * t;
		}

		/// <summary>
		/// 絶対値の小さな方を返す
		/// <param name="a">要素1</param>
		/// <param name="b">要素2</param>
		/// <returns>return: 結果</returns>
		/// </summary>
		public static float MinAbsf(float a, float b) {
			return Absf(a) < Absf(b) ? a : b;
		}

		// private
		static float Absf(float a) {
			return a < 0.0f ? -a : a;
		}
	}
}
