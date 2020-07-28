// Written by Alex Bedard-Reid <alexbedardreid@gmail.com>, 2017
using UnityEngine;
using System;
using System.Collections.Generic;
using StarSalvager.Utilities;

namespace Recycling
{
	public class Recycler : Singleton<Recycler>
	{
		private static Dictionary<Enum, RecycleBin> _enumDict = new Dictionary<Enum, RecycleBin>();
		private static Dictionary<Type, RecycleBin> _typeDict = new Dictionary<Type, RecycleBin>();


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

		public static void Recycle(Enum @enum, GameObject gameObject, params object[] args)
		{

			if (!_enumDict.TryGetValue(@enum, out var bin))
			{
				bin = new RecycleBin();
				_enumDict.Add(@enum, bin);

			}
			
			gameObject.GetComponent<ICustomRecycle>()?.CustomRecycle(args);

			bin.Store(gameObject);
			gameObject.transform.parent = transform;
			gameObject.transform.rotation = Quaternion.identity;

			if (gameObject.GetComponent<IRecycled>() is IRecycled recycled)
				recycled.IsRecycled = true;
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
		//FIXME I want to reduce the amount of GetComponent calls I'm doing here, as it feels like its getting a little crazy
		public static void Recycle(Type type, GameObject gameObject, params object[] args)
		{
			//See if object is Recyclable & is Already recycled
			//--------------------------------------------------------------------------------------------------------//
			
			var recycled = gameObject.GetComponent<IRecycled>();

			if (!(recycled is null) && recycled.IsRecycled)
			{
				Debug.Log($"{gameObject.name} is already recycled");
				return;
			}
			
			//Make sure that the gameObject actually contains the component we're trying to Recycle
			//--------------------------------------------------------------------------------------------------------//
			
			if(gameObject.GetComponent(type) is null)
				throw new NullReferenceException($"Unable to find {type.Name} on {gameObject.name}");
			
			//Get a Bin for the object
			//--------------------------------------------------------------------------------------------------------//
			
			if (!_typeDict.TryGetValue(type, out var bin))
			{
				bin = new RecycleBin();
				_typeDict.Add(type, bin);

			}

			//If the object wants to do anything special before being placed in the bin, Now is when we call that
			//--------------------------------------------------------------------------------------------------------//
			
			gameObject.GetComponent<ICustomRecycle>()?.CustomRecycle(args);

			//Officially recycle the object
			//--------------------------------------------------------------------------------------------------------//
			
			bin.Store(gameObject);
			gameObject.transform.parent = transform;
			gameObject.transform.rotation = Quaternion.identity;
			
			//If its a recyclable object, mark it as recycled
			//--------------------------------------------------------------------------------------------------------//
			
			if (recycled != null)
				recycled.IsRecycled = true;
			
			//--------------------------------------------------------------------------------------------------------//
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
			
			if (returnActive)
				gameObject.SetActive(true);
			
			if (gameObject.GetComponent<IRecycled>() is IRecycled recycled)
				recycled.IsRecycled = false;
			
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
			
			if (gameObject.GetComponent<IRecycled>() is IRecycled recycled)
				recycled.IsRecycled = false;

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
				throw new Exception($"{gameObject.name} has already been recycled. Ensure you add IRecycled to prevent this error");

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