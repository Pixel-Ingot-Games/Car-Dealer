using Gley.TrafficSystem.Internal;
using Gley.UrbanSystem.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Gley.TrafficSystem.Editor
{
    internal class LayerSetupWindow : SetupWindowBase
    {
        private LayerSetup _layerSetup;


        internal override SetupWindowBase Initialize(WindowProperties windowProperties, SettingsWindowBase window)
        {
            _layerSetup = FileCreator.LoadOrCreateLayers<LayerSetup>(TrafficSystemConstants.layerPath);
            return base.Initialize(windowProperties, window);
        }


        protected override void TopPart()
        {
            _layerSetup.roadLayers = LayerMaskField(new GUIContent("Road Layers", "Vehicle wheels will collide only with these layers"), _layerSetup.roadLayers);
            _layerSetup.trafficLayers = LayerMaskField(new GUIContent("Traffic Layers", "All traffic vehicles should be on this layer"), _layerSetup.trafficLayers);
            _layerSetup.buildingsLayers = LayerMaskField(new GUIContent("Buildings Layers", "Vehicles will try to avoid objects on these layers"), _layerSetup.buildingsLayers);
            _layerSetup.obstaclesLayers = LayerMaskField(new GUIContent("Obstacle Layers", "Vehicles will stop when objects on these layers are seen"), _layerSetup.obstaclesLayers);
            _layerSetup.playerLayers = LayerMaskField(new GUIContent("Player Layers", "Vehicles will stop when objects on these layers are seen"), _layerSetup.playerLayers);

            EditorGUILayout.Space();
            if (GUILayout.Button("Open Tags and Layers Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Tags and Layers");
            }

            base.TopPart();
        }


        private LayerMask LayerMaskField(GUIContent label, LayerMask layerMask)
        {
            LayerMask tempMask = EditorGUILayout.MaskField(label,InternalEditorUtility.LayerMaskToConcatenatedLayersMask(layerMask), InternalEditorUtility.layers);
            layerMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
            return layerMask;
        }


        internal override void DestroyWindow()
        {
            _layerSetup.edited = true;
            EditorUtility.SetDirty(_layerSetup);
            AssetDatabase.SaveAssets();
            SettingsWindow.UpdateLayers();
            base.DestroyWindow();
        }
    }
}