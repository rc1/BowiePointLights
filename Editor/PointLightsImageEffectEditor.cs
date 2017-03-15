﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BowiePointLights {

	[CustomEditor(typeof(PointLightsImageEffect))]
	public class PointLightsImageEffectEditor : Editor {

		public override void OnInspectorGUI () {

			var editor = target as PointLightsImageEffect;

			DrawDefaultInspector();

			EditorGUILayout.HelpBox( string.Format( "{0} Lights", editor.Count() ), MessageType.Info );
		}

	}

}
