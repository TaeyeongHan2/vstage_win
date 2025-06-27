using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AkilliMum.SRP.Mirror
{
	public class DynamicMirror : MonoBehaviour
	{
		[Tooltip("Dynamic mirror prefab to create on runtime")]
		public GameObject MirrorPrefab;
		[Tooltip("CameraShade instance which is added to camera")]
		public CameraShade CameraShadeInstance;

		private bool _alreadyCreated = false;

		//I will call this via button click from UI
		public void CreatePrefab()
		{
			if (_alreadyCreated)
				return;

			//create mirror object
			var mirrorObject = GameObject.Instantiate(MirrorPrefab);

			//create slots for mirror objects, cause i will use single plane as a mirror just creating
			//one slot
			CameraShadeInstance.Shades = new Shade[1]; //set size
			CameraShadeInstance.Shades[0] = new Shade(); //create new instance for first slot

			//set object to shade property; which is our prefab
			CameraShadeInstance.Shades[0].ObjectToShade = mirrorObject;
			//set the material; cause i know my material will be object's first material setting here
			CameraShadeInstance.Shades[0].MaterialToChange = mirrorObject.GetComponent<Renderer>().sharedMaterial;

			//you can set other values if you want here like intensity etc.
			//CameraShadeInstance.Intensity = 1;
			//CameraShadeInstance.MSAA = false;
			//CameraShadeInstance.DeviceType = DeviceType.Normal;
			//.... etc

			//Initialize the new Shades!
			CameraShadeInstance.InitializeProperties();

			//enable the script if not!
			CameraShadeInstance.IsEnabled = true;

			//fail safe not to create the prefab again and again :)
			_alreadyCreated = true;
		}
	}
}