using BepInEx;
using UnityEngine.SceneManagement;
using SaveSystemExtension;
using UnityEngine;
using System.Collections.Generic;
using HarmonyLib;

namespace SaveCubeExample
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, "1.0.0")]
    [BepInProcess("Mon Bazou.exe")]
    public class Plugin : BaseUnityPlugin
    {
        //Cubes...
        private List<MeshRenderer> cubeObjectList = new List<MeshRenderer>();

        //Recommended To Be Easily Accessible
        public static ModSaveData modSaveData;

        private void Awake()
        {
            //Create Save Data For API
            modSaveData = new ModSaveData(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_GUID);

            //Hook Events
            SaveTools.OnLoadItems += OnItemsLoad;
            SaveTools.OnSave += Save;
        }

        private void OnItemsLoad()
        {
            //Clear List To Prevent Using Null Values
            cubeObjectList.Clear();
            
            //Set And Add Variables To Save System Or Load Them, If They Already Exist.
            object cubeDataListObj = modSaveData.AddDataToSave(new List<CubeData>(), "VarCubeList");
            List<CubeData> cubeDataList = (List<CubeData>)cubeDataListObj;

            //Respawn Each Item From Last Save
            if (cubeDataList != null && cubeDataList.Count > 0)
            {
                foreach (CubeData cubeData in cubeDataList)
                {
                    CreateCube(cubeData.cubePosition, cubeData.cubeRotation, cubeData.cubeColor);
                }
            }
        }

        //After This Call The API Will Create A Save File For Each Mod And Apply The Data.
        private void Save()
        {
            List<CubeData> cubeDataList = new List<CubeData>();

            //Go Through The List Of Active Objects And Store Data
            foreach (MeshRenderer cubeObject in cubeObjectList)
            {
                //Create Cube Data
                CubeData cubeData = new CubeData();
                cubeData.cubeColor = cubeObject.material.color;
                cubeData.cubePosition = cubeObject.transform.position;
                cubeData.cubeRotation = cubeObject.transform.rotation;

                //Add Cube Data To Write List
                cubeDataList.Add(cubeData);
            }

            //Write List To Mod Object Data
            modSaveData.WriteDataToSave(cubeDataList, "VarCubeList");
        }

        private void Update()
        {
            //Return If Not In Game
            if (SceneManager.GetActiveScene().name != "Master") return;

            //Spawn Cube With Key Press (F1)
            if (UnityInput.Current.GetKeyDown(KeyCode.F1))
            {
                Color randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
                Transform player = Gameplay.i.Get_Player_Rigidbody.transform;
                Vector3 position = player.position + Vector3.up + player.forward;

                CreateCube(position, Quaternion.identity, randomColor);
            }
        }

        private void CreateCube(Vector3 position, Quaternion rotation, Color cubeColor)
        {
            //Spawn Cube
            GameObject cubeVariable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubeVariable.transform.localScale = new Vector3(0.5f, 0.1f, 0.5f);
            cubeVariable.layer = LayerMask.NameToLayer("InteractableItem");

            //Set Position And Rotation Of Transform
            cubeVariable.transform.position = position;
            cubeVariable.transform.rotation = rotation;

            //Create Interactable Component
            InteractableItems cubeInteractable = cubeVariable.AddComponent<InteractableItems>();
            Traverse.Create(cubeInteractable).Field("Mass").SetValue(20f);
            cubeInteractable.Name = "Cube";

            //Create Paintable Panel Component
            MeshRenderer meshRenderer = cubeVariable.GetComponent<MeshRenderer>();
            Vehicle_PaintablePanel panel = cubeVariable.AddComponent<Vehicle_PaintablePanel>();
            PaintablePanel_Informations cubePaintInfo = new PaintablePanel_Informations(meshRenderer);
            PaintablePanel_Informations[] paintInfoArray = { cubePaintInfo };
            Traverse.Create(panel).Field("PannelsRelated").SetValue(paintInfoArray);

            //Set Color Of Mesh
            cubePaintInfo.mesh.material.color = cubeColor;

            //Here To Stop Errors
            GameObject dumbObject = new GameObject();
            dumbObject.transform.parent = toolObject.transform;
            dumbObject.transform.localPosition = Vector3.zero;
            GameObject[] storedArray = { dumbObject };
            Traverse.Create(cubeInteractable).Field("ToDisableWhenGrab").SetValue(storedArray);
            cubeInteractable.meshRenderer = cubeVariable.GetComponent<MeshRenderer>();

            //MUST HAVE IT FOR INTERACTABLES/VEHICLES TO IGNORE BUILT IN SAVE SYSTEM FOR NO ERRORS ON LOADING!!!
            SaveTools.BlockFromMainSave(cubeInteractable);

            //Add Object To List
            cubeObjectList.Add(meshRenderer);
        }
    }
}
