﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class SpawnExtension
    {
        #region Properties

        SerializedProperty autoSimulationType;
        SerializedProperty autoSimulationHeightOffset;

        #endregion Properties

#pragma warning disable 0414
        PrefabPainterEditor editor;
#pragma warning restore 0414

        PrefabPainter editorTarget;

        public SpawnExtension(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.editorTarget = editor.GetPainter();

            autoSimulationType = editor.FindProperty(x => x.spawnSettings.autoSimulationType);
            autoSimulationHeightOffset = editor.FindProperty(x => x.spawnSettings.autoSimulationHeightOffset);
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Physics", GUIStyles.BoxTitleStyle);

            // auto physics
            EditorGUILayout.PropertyField(autoSimulationType, new GUIContent("Simulation"));
            if (autoSimulationType.enumValueIndex != (int)SpawnSettings.AutoSimulationType.None)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(autoSimulationHeightOffset, new GUIContent("Height Offset"));
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
        }

       
    }
}