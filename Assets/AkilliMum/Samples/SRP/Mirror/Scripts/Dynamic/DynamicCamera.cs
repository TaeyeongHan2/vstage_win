using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkilliMum.SRP.Mirror
{
	public class DynamicCamera : MonoBehaviour
	{
		[Tooltip("Camera prefab to create on runtime")]
		public GameObject CameraPrefab;
		[Tooltip("Mirror object to add to CameraShade (Camera)")]
		public GameObject MirrorObject;

		private bool _alreadyCreated = false;

		//I will call this via button click from UI
		public void CreatePrefab()
		{
			if (_alreadyCreated)
				return;

			//create camera
			var camera = GameObject.Instantiate(CameraPrefab);

			//get CameraShade script from camera
			var cameraShadeInstance = camera.GetComponentInChildren<CameraShade>();

			//create slots for mirror objects, cause i will use single plane as a mirror just creating
			//one slot
			cameraShadeInstance.Shades = new Shade[1]; //set size
			cameraShadeInstance.Shades[0] = new Shade(); //create new instance for first slot

			//set object to shade property; which is our mirror object
			cameraShadeInstance.Shades[0].ObjectToShade = MirrorObject;
			//set the material; cause i know my material will be object's first material setting here
			cameraShadeInstance.Shades[0].MaterialToChange = MirrorObject.GetComponent<Renderer>().sharedMaterial;

			//you can set other values if you want here like intensity etc.
			//cameraShadeInstance.Intensity = 1;
			//cameraShadeInstance.MSAA = false;
			//cameraShadeInstance.DeviceType = DeviceType.Normal;
			//.... etc

			//Initialize the new Shades!
			cameraShadeInstance.InitializeProperties();

			//enable the script if not!
			cameraShadeInstance.IsEnabled = true;

			//fail safe not to create the prefab again and again :)
			_alreadyCreated = true;
		}
	}
}