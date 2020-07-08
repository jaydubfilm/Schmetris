// Written by Alex Bedard-Reid <alexbedardreid@gmail.com>, 2017

using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StarSalvager;
using StarSalvager.Utilities;
using UnityEngine.Assertions;

namespace Recycling
{
	public class Recycler : Singleton<Recycler>
	{
		private static Dictionary<Enum, RecycleBin> _enumDict = new Dictionary<Enum, RecycleBin>();
		private static Dictionary<Type, RecycleBin> _typeDict = new Dictionary<Type, RecycleBin>();


		private new static Transform Transform
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

		private static void Recycle(Enum @enum, GameObject gameObject, params object[] args)
		{

			if (!_enumDict.TryGetValue(@enum, out var bin))
			{
				bin = new RecycleBin();
				_enumDict.Add(@enum, bin);

			}
			
			gameObject.GetComponent<ICustomRecycle>()?.CustomRecycle(args);

			bin.Store(gameObject);
			gameObject.transform.parent = Transform;
			gameObject.transform.rotation = Quaternion.identity;
			
			
			gameObject.GetComponent<IRecycled>()?.OnRecycled();
		}
		public static bool TryGrab(Enum @enum, out GameObject gameObject, bool returnActive = true)
		{
			gameObject = null;

			if (!_enumDict.TryGetValue(@enum, out var bin)) 
				return false;
			if (!bin.Grab(out gameObject)) 
				return false;
			if (returnActive) gameObject.SetActive(true);
				return true;
		}
		
		public static bool TryGrab<T>(Enum @enum, out T monoBehaviour, bool returnActive = true) where T: MonoBehaviour
		{
			monoBehaviour = null;

			if (!_enumDict.TryGetValue(@enum, out var bin)) 
				return false;
			if (!bin.Grab(out var gameObject)) 
				return false;
			
			if (returnActive) 
				gameObject.SetActive(true);

			monoBehaviour = gameObject.GetComponent<T>();
			
			return true;
		}
		
		//============================================================================================================//
		public static void Recycle(Type type, GameObject gameObject, params object[] args)
		{
			if(gameObject.GetComponent(type) is null)
				throw new NullReferenceException($"Unable to find {type.Name} on {gameObject.name}");
			
			if (!_typeDict.TryGetValue(type, out var bin))
			{
				bin = new RecycleBin();
				_typeDict.Add(type, bin);

			}

			gameObject.GetComponent<ICustomRecycle>()?.CustomRecycle(args);

			bin.Store(gameObject);
			gameObject.transform.parent = Transform;
			gameObject.transform.rotation = Quaternion.identity;
			
			
			gameObject.GetComponent<IRecycled>()?.OnRecycled();
			
		}
		
		public static void Recycle<T>(GameObject gameObject, params object[] args)
		{
			Recycle(typeof(T), gameObject,args);
		}
		public static void Recycle<T>(MonoBehaviour mono, params object[] args)
		{
			Recycle(typeof(T), mono.gameObject,args);
		}

		public static bool TryGrab(Type type, out GameObject gameObject, bool returnActive = true)
		{
			gameObject = null;

			if (!_typeDict.TryGetValue(type, out var bin)) 
				return false;
			
			if (!bin.Grab(out gameObject)) 
				return false;
			
			if(gameObject.GetComponent(type) is null)
				throw new NullReferenceException($"Unable to find {type.Name} on {gameObject.name}");
			
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

			if (!_typeDict.TryGetValue(typeof(T), out var bin)) 
				return false;
			
			if (!bin.Grab(out var gameObject)) 
				return false;
			
			if (returnActive) gameObject.SetActive(true);

			monoBehaviour = gameObject.GetComponent<T>();
			
			if(monoBehaviour is null)
				throw new NullReferenceException($"Unable to find {typeof(T).Name} on {gameObject.name}");

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
			
			if(_recycled.Contains(gameObject))
				throw new Exception($"{gameObject.name} has already been recycled");

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