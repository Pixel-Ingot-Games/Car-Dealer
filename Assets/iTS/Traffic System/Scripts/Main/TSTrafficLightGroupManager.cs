using UnityEngine;
using System.Collections;
using ITS.Utils;

public class TSTrafficLightGroupManager : MonoBehaviour {
	public float greenLightTime = 25f;
	public float yellowLightTime = 5f;
	
	
	public static void AutoSyncTrafficLights(TSTrafficLight[] trafficLights, float greenLightTime, float yellowLightTime)
	{
		int t = 0;
		int totalAmountOfLights = trafficLights.Length;
		foreach(TSTrafficLight tLight in trafficLights)
		{
			if (t ==0)
			{
				//First traffic light
				CheckAndCreateMissingLights(tLight,false, greenLightTime);
				AssignLightsTimes(tLight, true, false, totalAmountOfLights,t, greenLightTime, yellowLightTime);
			}else if (t < trafficLights.Length-1)
			{
				//Middle traffic lights
				CheckAndCreateMissingLights(tLight,true, greenLightTime);
				AssignLightsTimes(tLight, false, true, totalAmountOfLights,t, greenLightTime, yellowLightTime);

			}else{
				//Final traffic light
				CheckAndCreateMissingLights(tLight,false, greenLightTime);
				AssignLightsTimes(tLight, false, false, totalAmountOfLights,t, greenLightTime, yellowLightTime);
			}
			t++;

		}

	}


	/// <summary>
	/// Checks the and create missing lights.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="inBetween">If set to <c>true</c> in between.</param>
	private static void AssignLightsTimes(TSTrafficLight tLight,bool first, bool inBetween, int totalLights, int currentLight, float greenLightTime, float yellowLightTime){


		if (first)
		{
			AssignFirstLightTimes(tLight,totalLights, greenLightTime, yellowLightTime);

		}else if (inBetween)
		{
			AssignMiddleLightTimes(tLight,totalLights,currentLight, greenLightTime, yellowLightTime);
		}
		else if (!first)
		{
			AssignLastLightTimes(tLight,totalLights, greenLightTime, yellowLightTime);
		}
	}

	/// <summary>
	/// Assigns the first light times.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="totalLights">Total lights.</param>
	private static void AssignFirstLightTimes(TSTrafficLight tLight, int totalLights, float greenLightTime, float yellowLightTime)
	{
		bool arranged = false;
		int t = 0;
		while(!arranged)
		{
			for (int i =0;i < tLight.lights.Count;i++)
			{
				switch(t)
				{
					//green light
				case 0:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Green)
					{
						tLight.lights[i].lightTime = greenLightTime;
						tLight.lights.Move(i,0);
						t = 1;
					}
					break;
					//yellow light
				case 1:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Yellow)
					{
						tLight.lights[i].lightTime = yellowLightTime;
						tLight.lights.Move(i,1);
						t = 2;
					}
					break;
					//red light
				case 2:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Red)
					{
						tLight.lights[i].lightTime = (greenLightTime + yellowLightTime) * (totalLights-1);
						tLight.lights.Move(i,2);
						t = 3;
					}
					
					break;
				}
			}
			if (t == 3) arranged = true;
		}
	}

	/// <summary>
	/// Assigns the last light times.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="totalLights">Total lights.</param>
	private static void AssignLastLightTimes(TSTrafficLight tLight, int totalLights, float greenLightTime, float yellowLightTime)
	{
		bool arranged = false;
		int t = 0;
		while(!arranged)
		{
			for (int i =0;i < tLight.lights.Count;i++)
			{
				switch(t)
				{
				case 0:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Red)
					{
						tLight.lights[i].lightTime = (greenLightTime + yellowLightTime) * (totalLights-1);
						tLight.lights.Move(i,0);
						t = 1;
					}
					break;
					//green light
				case 1:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Green)
					{
						tLight.lights[i].lightTime = greenLightTime;
						tLight.lights.Move(i,1);
						t = 2;
					}
					break;
					//yellow light
				case 2:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Yellow)
					{
						tLight.lights[i].lightTime = yellowLightTime;
						tLight.lights.Move(i,2);
						t = 3;
					}
					break;
				}
			}
			if (t == 3) arranged = true;
		}
	}

	/// <summary>
	/// Assigns the middle light times.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="totalLights">Total lights.</param>
	/// <param name="currentLight">Current light.</param>
	private static void AssignMiddleLightTimes(TSTrafficLight tLight, int totalLights, int currentLight, float greenLightTime, float yellowLightTime)
	{
		bool arranged = false;
		int t = 0;
		while(!arranged)
		{
			for (int i =0;i < tLight.lights.Count;i++)
			{
				switch(t)
				{
					//red light
				case 0:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Red)
					{
						tLight.lights[i].lightTime = (greenLightTime + yellowLightTime) * (currentLight);
						tLight.lights.Move(i,0);
						t = 1;
					}
					
					break;
					//green light
				case 1:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Green)
					{
						tLight.lights[i].lightTime = greenLightTime;
						tLight.lights.Move(i,1);
						t = 2;
					}
					break;
					//yellow light
				case 2:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Yellow)
					{
						tLight.lights[i].lightTime = yellowLightTime;
						tLight.lights.Move(i,2);
						t = 3;
					}
					break;
					//red light
				case 3:
					if (tLight.lights[i].lightType == TSTrafficLight.LightType.Red)
					{
						tLight.lights[i].lightTime = (greenLightTime + yellowLightTime) * (totalLights-1-currentLight);
						tLight.lights.Move(i,3);
						t = 4;
					}
					
					break;
				}
			}
			if (t == 4) arranged = true;
		}
	}



	/// <summary>
	/// Checks the and create missing lights.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="inBetween">If set to <c>true</c> in between.</param>
	private static void CheckAndCreateMissingLights(TSTrafficLight tLight, bool inBetween, float greenLightTime){
		int numberOfRedLights = 0;
		if (inBetween)
		{
			numberOfRedLights = 2;
		}
		else
		{
			numberOfRedLights = 1;
		}
		//Red lights
		int actualNumberOfLights = NumberOfLightsByType(tLight,TSTrafficLight.LightType.Red);
		if (actualNumberOfLights != numberOfRedLights)
		{
			if (actualNumberOfLights > numberOfRedLights)
			{
				RemoveLightsByType(tLight,TSTrafficLight.LightType.Red, actualNumberOfLights-numberOfRedLights);
			}else if (actualNumberOfLights < numberOfRedLights)
			{
				AddLightsByType(tLight,TSTrafficLight.LightType.Red,numberOfRedLights-actualNumberOfLights);
			}
			
		}
		//Other Lights
		CheckOtherTypesOfLights(ref tLight, greenLightTime);
	}

	/// <summary>
	/// Checks the other types of lights.
	/// </summary>
	/// <param name="tLight">T light.</param>
	private static void CheckOtherTypesOfLights(ref TSTrafficLight tLight, float greenLightTime)
	{
		TSTrafficLight.LightType lightType = TSTrafficLight.LightType.Green;

		for (int i = 0; i < 2;i++)
		{
			switch(i)
			{
			case 1:
				lightType = TSTrafficLight.LightType.Yellow;

				break;
			}
			int actualNumberOfLights = NumberOfLightsByType(tLight,lightType);
			if (greenLightTime != 0 && actualNumberOfLights != 1)
			{
				if (actualNumberOfLights > 1)
				{
					RemoveLightsByType(tLight,lightType, actualNumberOfLights-1);
				}else if (actualNumberOfLights < 1)
				{
					AddLightsByType(tLight,lightType,1);
				}
				
			}
		}
	}

	/// <summary>
	/// Numbers the type of the of lights by.
	/// </summary>
	/// <returns>The of lights by type.</returns>
	private static int NumberOfLightsByType(TSTrafficLight tLight, TSTrafficLight.LightType lightType)
	{
		int result = 0;
		foreach (TSTrafficLight.TSLight light in tLight.lights)
		{
			if (light.lightType == lightType)
				result++;
		}
		return result;
	}

	/// <summary>
	/// Removes the type of the lights by.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="lightType">Light type.</param>
	/// <param name="amount">Amount.</param>
	private static void RemoveLightsByType(TSTrafficLight tLight, TSTrafficLight.LightType lightType, int amount)
	{
		int result = 0;
		TSTrafficLight.TSLight light = null;
		for (int i = 0; i < tLight.lights.Count;i++)
		{
			light = tLight.lights[i];
			if (result == amount)break;
			if (light.lightType == lightType)
				tLight.lights.Remove(light);
			result++;
		}
		
	}

	/// <summary>
	/// Adds the type of the lights by.
	/// </summary>
	/// <param name="tLight">T light.</param>
	/// <param name="lightType">Light type.</param>
	/// <param name="amount">Amount.</param>
	private static void AddLightsByType(TSTrafficLight tLight, TSTrafficLight.LightType lightType, int amount)
	{
		TSTrafficLight.TSLight clone = null;
		for (int i = 0; i < tLight.lights.Count;i++){
			if (tLight.lights[i].lightType == lightType)
			{
				clone = tLight.lights[i];
			}
		}

		for (int i = 0; i < amount;i++)
		{
			AddeNewLight(ref tLight);
			tLight.lights[tLight.lights.Count-1].lightType = lightType;
			if (clone !=null)
			{
				tLight.lights[tLight.lights.Count-1].enableDisableRenderer = clone.enableDisableRenderer;
				tLight.lights[tLight.lights.Count-1].lightGameObject = clone.lightGameObject;
				tLight.lights[tLight.lights.Count-1].lightMeshRenderer = clone.lightMeshRenderer;
				tLight.lights[tLight.lights.Count-1].lightTexture = clone.lightTexture;
				tLight.lights[tLight.lights.Count-1].shaderTexturePropertyName = clone.shaderTexturePropertyName;

			}
		}
		
	}
	
	private static void AddeNewLight(ref TSTrafficLight tLight)
	{
		tLight.lights.Add(new TSTrafficLight.TSLight());
		tLight.lights[tLight.lights.Count-1].lightTime = 15;
		tLight.lights[tLight.lights.Count-1].shaderTexturePropertyName = "_MainTex";
	}

}
