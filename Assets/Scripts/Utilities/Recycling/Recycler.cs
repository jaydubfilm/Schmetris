﻿// Written by Alex Bedard-Reid <alexbedardreid@gmail.com>, 2017

using UnityEngine;
using System;
using System.Collections.Generic;
using StarSalvager.Utilities;

namespace Recycling
{
	public class Recycler : SceneSingleton<Recycler>
	{
		private static Dictionary<Enum, RecycleBin> _enumDict = new Dictionary<Enum, RecycleBin>();
		private static Dictionary<Type, RecycleBin> _typeDict = new Dictionary<Type, RecycleBin>();
		private static RecycleBin _bin;


		private new static Transform transform
		{
			get
			{
				if (_transform == null)
					_transform = Instance.gameObject.transform;

				return _transform;
			}
		}
		private static Transform _transform;

		//============================================================================================================//

		protected override void OnDestroy()
		{
			base.OnDestroy();
			
			_typeDict.Clear();
			_transform = null;
		}

		//============================================================================================================//

		public static void Recycle(Enum @enum, GameObject gameObject)
		{

			if (!_enumDict.TryGetValue(@enum, out _bin))
			{
				_bin = new RecycleBin();
				_enumDict.Add(@enum, _bin);

			}

			_bin.Store(gameObject);
			gameObject.transform.parent = transform;
			
			gameObject.GetComponent<IRecycle>()?.OnRecycle();
		}
		public static bool TryGrab(Enum @enum, out GameObject gameObject, bool returnActive = true)
		{
			gameObject = null;

			if (!_enumDict.TryGetValue(@enum, out _bin)) 
				return false;
			if (!_bin.Grab(out gameObject)) 
				return false;
			if (returnActive) gameObject.SetActive(true);
				return true;
		}
		
		public static bool TryGrab<T>(Enum @enum, out T monoBehaviour, bool returnActive = true) where T: MonoBehaviour
		{
			monoBehaviour = null;

			if (!_enumDict.TryGetValue(@enum, out _bin)) 
				return false;
			if (!_bin.Grab(out var gameObject)) 
				return false;
			
			if (returnActive) 
				gameObject.SetActive(true);

			monoBehaviour = gameObject.GetComponent<T>();
			
			return true;
		}
		
		//============================================================================================================//

		public static void Recycle(Type type, GameObject gameObject)
		{

			if (!_typeDict.TryGetValue(type, out _bin))
			{
				_bin = new RecycleBin();
				_typeDict.Add(type, _bin);

			}

			_bin.Store(gameObject);
			gameObject.transform.parent = transform;
			
			gameObject.GetComponent<IRecycle>()?.OnRecycle();
		}
		
		public static void Recycle<T>(GameObject gameObject)
		{
			Recycle(typeof(T), gameObject);
		}

		
		
		public static bool TryGrab(Type type, out GameObject gameObject, bool returnActive = true)
		{
			gameObject = null;

			if (!_typeDict.TryGetValue(type, out _bin)) 
				return false;
			
			if (!_bin.Grab(out gameObject)) 
				return false;
			
			if (returnActive) gameObject.SetActive(true);
				return true;
		}
		
		public static bool TryGrab<T>(out GameObject gameObject, bool returnActive = true)
		{
			return TryGrab(typeof(T), out gameObject, returnActive);
		}
		
		public static bool TryGrab<T>(out T monoBehaviour, bool returnActive = true) where T: MonoBehaviour
		{
			monoBehaviour = null;

			if (!_typeDict.TryGetValue(typeof(T), out _bin)) 
				return false;
			
			if (!_bin.Grab(out var gameObject)) 
				return false;
			
			if (returnActive) gameObject.SetActive(true);

			monoBehaviour = gameObject.GetComponent<T>();
			
			return true;
		}
	}

	//============================================================================================================//

	internal class RecycleBin
	{
		private Stack<GameObject> _recycled;

		public void Store(GameObject gameObject)
		{
			if (_recycled == null) _recycled = new Stack<GameObject>();

			gameObject.SetActive(false);
			_recycled.Push(gameObject);

		}

		public bool Grab(out GameObject gameObject)
		{
			if (_recycled == null || _recycled.Count <= 0)
			{
				gameObject = null;
				return false;
			}
			gameObject = _recycled.Pop();
			return true;
		}


	}
}