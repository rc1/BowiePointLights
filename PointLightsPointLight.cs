using UnityEngine;
using BowieCode;

namespace BowiePointLights {

	[ExecuteInEditMode]
	public class PointLightsPointLight : MonoBehaviour {

		[DisableOnPlay]
		public bool addToDefault = true;

		public Color color;
		[Range( 0.0f, 1.0f )]
		public float intensity;
		public float range = 1.0f;

		private Transform _transform;

		public Vector3 position {
			get {
				if ( _transform == null ) {
					_transform = GetComponent<Transform>();
				}
				return _transform.position;
			}
		}

		void Awake () {
			AddOrRemovePointLight();
		}

		void OnValidate () {
			AddOrRemovePointLight();
		}

		void OnEnable () {
			AddOrRemovePointLight();
		}

		void OnDisable () {
			AddOrRemovePointLight();
		}

		void OnDestroy () {
			RemoveFromDefaultPointLights();
		}

		void AddOrRemovePointLight () {
			if ( addToDefault ) {
				AddToDefaultPointLights();
			} else {
				RemoveFromDefaultPointLights();
			}
		}

		void AddToDefaultPointLights () {
			PointLightsImageEffect imageEffect = DefaultInstance<PointLightsImageEffect>.Get();
			if ( imageEffect != null ) {
				imageEffect.Add( this );
			}
		}

		void RemoveFromDefaultPointLights () {
			PointLightsImageEffect imageEffect = DefaultInstance<PointLightsImageEffect>.Get();
			if ( imageEffect != null ) {
				imageEffect.Remove( this );
			}
		}

	}

}
