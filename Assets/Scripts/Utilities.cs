using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
	public Utilities(){
		
	}

	public float ArraySum(float[] array)
	{
		float sum = 0;
		for (int i = 0; i < array.Length; i++)
		{
			sum += array[i];
		}
		return sum;
	}
}
