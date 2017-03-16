using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using BowieCode;

namespace BowiePointLights {

	[ExecuteInEditMode]
	public class PointLightsImageEffect : MonoBehaviour {

		private Camera _camera;

		public bool isDefault = false;

		public Material effectMaterial;

		private const int MAX_LIGHTS = 200;

		private Vector4[] _lightPositions = new Vector4[ MAX_LIGHTS ];
		private float[] _lightIntensities = new float[ MAX_LIGHTS ];
		private float[] _lightRanges = new float[ MAX_LIGHTS ];
		private Color[] _lightColors = new Color[ MAX_LIGHTS ];

		private HashSet<PointLightsPointLight> pointLightTargets = new HashSet<PointLightsPointLight>();

		void OnValidate () {
			if ( isDefault ) {
				DefaultInstance<PointLightsImageEffect>.Set( this );
			} else {
				DefaultInstance<PointLightsImageEffect>.Unset( this );
			}
		}

		void Awake () {
			if ( isDefault ) {
				DefaultInstance<PointLightsImageEffect>.Set( this );
			}
		}

		void OnEnable () {
			_camera = GetComponent<Camera>();
			_camera.depthTextureMode = DepthTextureMode.DepthNormals;
		}

		public int Count () {
			return pointLightTargets.Count;
		}

		void UpdateLights () {

			pointLightTargets.Take( MAX_LIGHTS ).ForEachWithIndex( ( pointLight, idx ) => {
				_lightPositions[ idx ] = pointLight.position;
				_lightIntensities[ idx ] = pointLight.intensity;
				_lightRanges[ idx ] = pointLight.range;
				_lightColors[ idx ] = pointLight.color;
			} );

			for ( int idx = pointLightTargets.Count; idx < MAX_LIGHTS; idx++ ) {
				_lightIntensities[ idx ] = 0.0f;
			}
		}

		public void Add ( PointLightsPointLight pointLight ) {
			pointLightTargets.Add( pointLight );
		}

		public void Remove ( PointLightsPointLight pointLight ) {
			pointLightTargets.Remove( pointLight );
		}

		[ImageEffectOpaque]
		void OnRenderImage ( RenderTexture src, RenderTexture dst ) {

			UpdateLights();

			// Get the camera matrix to convert the view space normals to world normals
			Matrix4x4 MV = _camera.cameraToWorldMatrix;
			effectMaterial.SetMatrix( "_CameraMV", MV );

			effectMaterial.SetVectorArray( "_LightPositions", _lightPositions );
			effectMaterial.SetColorArray( "_LightColors", _lightColors );
			effectMaterial.SetFloatArray( "_LightRanges", _lightRanges );
			effectMaterial.SetFloatArray( "_LightIntensitys", _lightIntensities );

			RaycastCornerBlit( src, dst, effectMaterial );
		}

		void RaycastCornerBlit ( RenderTexture source, RenderTexture dest, Material mat ) {
			// Compute Frustum Corners
			float camFar = _camera.farClipPlane;
			float camFov = _camera.fieldOfView;
			float camAspect = _camera.aspect;

			float fovWHalf = camFov * 0.5f;

			Vector3 toRight = _camera.transform.right * Mathf.Tan( fovWHalf * Mathf.Deg2Rad ) * camAspect;
			Vector3 toTop = _camera.transform.up * Mathf.Tan( fovWHalf * Mathf.Deg2Rad );

			Vector3 topLeft = ( _camera.transform.forward - toRight + toTop );
			float camScale = topLeft.magnitude * camFar;

			topLeft.Normalize();
			topLeft *= camScale;

			Vector3 topRight = ( _camera.transform.forward + toRight + toTop );
			topRight.Normalize();
			topRight *= camScale;

			Vector3 bottomRight = ( _camera.transform.forward + toRight - toTop );
			bottomRight.Normalize();
			bottomRight *= camScale;

			Vector3 bottomLeft = ( _camera.transform.forward - toRight - toTop );
			bottomLeft.Normalize();
			bottomLeft *= camScale;

			// Custom Blit, encoding Frustum Corners as additional Texture Coordinates
			RenderTexture.active = dest;

			mat.SetTexture( "_MainTex", source );

			GL.PushMatrix();
			GL.LoadOrtho();

			mat.SetPass( 0 );

			GL.Begin( GL.QUADS );

			GL.MultiTexCoord2( 0, 0.0f, 0.0f );
			GL.MultiTexCoord( 1, bottomLeft );
			GL.Vertex3( 0.0f, 0.0f, 0.0f );

			GL.MultiTexCoord2( 0, 1.0f, 0.0f );
			GL.MultiTexCoord( 1, bottomRight );
			GL.Vertex3( 1.0f, 0.0f, 0.0f );

			GL.MultiTexCoord2( 0, 1.0f, 1.0f );
			GL.MultiTexCoord( 1, topRight );
			GL.Vertex3( 1.0f, 1.0f, 0.0f );

			GL.MultiTexCoord2( 0, 0.0f, 1.0f );
			GL.MultiTexCoord( 1, topLeft );
			GL.Vertex3( 0.0f, 1.0f, 0.0f );

			GL.End();
			GL.PopMatrix();
		}
	}


}