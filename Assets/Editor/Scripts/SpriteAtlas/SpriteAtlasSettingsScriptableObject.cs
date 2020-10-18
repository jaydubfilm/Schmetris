using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.U2D;

namespace StarSalvager.Editor.CustomEditors
{
    [CreateAssetMenu(fileName = "Sprite Atlas Settings", menuName = "Star Salvager/Sprite Atlas/Settings Asset")]
    public class SpriteAtlasSettingsScriptableObject : ScriptableObject
    {
        
        public SpriteAtlas partsAtlas;
        public SpriteAtlas bitsAtlas;


        [FolderPath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Sprites/")]
        public string partSpritePath;

        [FolderPath(AbsolutePath = true, RequireExistingPath = true, ParentFolder = "Assets/Sprites/")]
        public string bitSpritePath;

        [HideInInspector]
        public List<PartPath> PartPaths = new List<PartPath>();
        [HideInInspector]
        public List<BitPath> BitPaths = new List<BitPath>();
        [HideInInspector]
        public List<ComponentPath> ComponentPaths = new List<ComponentPath>();

        public string GetTypePath<TE>(TE type) where TE: Enum
        {
            string path;
            
            switch (type)
            {
                case PART_TYPE _:
                    path = Path.Combine(partSpritePath, type.ToString());
                    break;
                case BIT_TYPE _:
                    path = Path.Combine(bitSpritePath, type.ToString());
                    break;
                case COMPONENT_TYPE _:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return path;
        }

        public string GetDataPath<TE>(TE type) where TE : Enum
        {
            switch (type)
            {
                case PART_TYPE partType:
                    return PartPaths.FirstOrDefault(x => x.Type == partType)?.path;
                
                case BIT_TYPE bitType:
                    return BitPaths.FirstOrDefault(x => x.Type == bitType)?.path;
                
                case COMPONENT_TYPE componentType:
                    return ComponentPaths.FirstOrDefault(x => x.Type == componentType)?.path;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public void UpdatePath<TE>(TE type, string newPath) where TE : Enum
        {
            switch (type)
            {
                case PART_TYPE partType:
                    UpdateAddEntry(ref PartPaths, partType, newPath);
                    break;
                
                case BIT_TYPE bitType:
                    UpdateAddEntry(ref BitPaths, bitType, newPath);
                    break;
                
                case COMPONENT_TYPE componentType:
                    UpdateAddEntry(ref ComponentPaths, componentType, newPath);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void UpdateAddEntry<T, TE>(ref List<T> dataPaths, TE type, string newPath)
            where T : DataPathBase<TE>, new() where TE : Enum
        {
            var index = dataPaths.FindIndex(x => Equals(x.Type, type));

            if (index >= 0)
            {
                PartPaths[index].path = newPath;
                return;
            }
                    
            dataPaths.Add(new T
            {
                Type = type,
                path = newPath
            });
        }
    }

    //This needs to be structured like this to allow things to be saved through the Unity Inspector
    [Serializable]
    public class PartPath : DataPathBase<PART_TYPE> { }
    
    [Serializable]
    public class BitPath : DataPathBase<BIT_TYPE> { }
    [Serializable]
    public class ComponentPath : DataPathBase<COMPONENT_TYPE> { }

    [Serializable]
    public abstract class DataPathBase<TE> : IEquatable<DataPathBase<TE>> where TE : Enum
    {
        public TE Type;
        public string path;

        #region IEquatable

        public bool Equals(DataPathBase<TE> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TE>.Default.Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DataPathBase<TE>) obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TE>.Default.GetHashCode(Type);
        }

        #endregion //IEquatable
    }

}
