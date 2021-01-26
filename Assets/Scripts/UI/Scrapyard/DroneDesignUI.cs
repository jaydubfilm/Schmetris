using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.SceneManagement;
using StarSalvager.Utilities.UI;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class DroneDesignUI : MonoBehaviour
    {
        public bool CanAffordRepair { get; private set; }

        [SerializeField, Required] private PointerEvents repairButtonPointerEvents;

        //====================================================================================================================//

        [SerializeField] private PurchasePatchUIElementScrollView purchasePatchUIElementScrollView;

        //============================================================================================================//

        [Serializable]
        public struct PatchUI
        {
            [HorizontalGroup("Row1"), LabelWidth(75)]
            public Image backgroundImage;

            [HorizontalGroup("Row1"), LabelWidth(50)]
            public TMP_Text text;
        }

        [Serializable]
        public struct GradeUI
        {
            [HorizontalGroup("Row1"), LabelWidth(75)]
            public Image bitImage;

            [HorizontalGroup("Row1"), LabelWidth(50)]
            public TMP_Text text;
        }

        [SerializeField, FoldoutGroup("Part Details Window")]
        private RectTransform partDetailsContainerRectTransform;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private Image partImage;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partNameText;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text otherPartDetailsText;

        [FormerlySerializedAs("PatchUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        private PatchUI[] patchUis;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partDetailsText;

        [FormerlySerializedAs("GradeUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        private GradeUI[] gradeUis;


        //====================================================================================================================//


        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button repairButton;

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private FadeUIImage repairButtonGlow;

        [SerializeField, Required, BoxGroup("Menu Buttons")]
        private Button launchButton;

        private TMP_Text _repairButtonText;

        [SerializeField] private CameraController CameraController;


        public static Action CheckBlueprintNewAlertUpdate;

        //============================================================================================================//

        private DroneDesigner DroneDesigner
        {
            get
            {
                if (_droneDesigner == null)
                    _droneDesigner = FindObjectOfType<DroneDesigner>();

                return _droneDesigner;
            }
        }

        [SerializeField, Required] private DroneDesigner _droneDesigner;

        private ScrapyardLayout currentSelected;

        public bool IsPopupActive => Alert.Displayed;
        private bool _currentlyOverwriting;

        private bool _scrollViewsSetup;

        float cameraScaleOnEnter = 71;

        //Unity Functions
        //============================================================================================================//

        #region Unity Functions

        private void Start()
        {
            InitButtons();

            _scrollViewsSetup = true;

            _currentlyOverwriting = false;


            InitPurchasePatches();

            ShowPartDetails(false, null);
        }

        private void OnEnable()
        {
            Camera.onPostRender += _droneDesigner.DrawGL;
        }

        private void OnDisable()
        {
            Camera.onPostRender -= _droneDesigner.DrawGL;
            _droneDesigner.RecycleDrone();

            Globals.ScaleCamera(Globals.CameraScaleSize);
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init

        private void InitButtons()
        {
            _repairButtonText = repairButton.GetComponentInChildren<TMP_Text>();

            repairButton.onClick.AddListener(() =>
            {
                /*DroneDesigner.RepairParts();
                PreviewRepairCost(false);*/
            });

            launchButton.onClick.AddListener(() =>
            {
                SceneLoader.ActivateScene(SceneLoader.UNIVERSE_MAP, SceneLoader.SCRAPYARD);
            });

            //--------------------------------------------------------------------------------------------------------//

        }

        #endregion //Init

        //============================================================================================================//

        #region Scroll Views

        private void InitPurchasePatches()
        {
            var patches = new[]
            {
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.RANGE
                    }
                },
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.DAMAGE
                    }
                },
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.FIRE_RATE
                    }
                },
                new Purchase_PatchData
                {
                    cost = 10,
                    PatchData = new PatchData
                    {
                        Level = 0,
                        Type = (int) PATCH_TYPE.GRADE
                    }
                }
            };

            foreach (var t in patches)
            {
                var element = purchasePatchUIElementScrollView.AddElement(t);
                element.Init(t);
            }
        }

        #endregion //Scroll Views

        //====================================================================================================================//

        #region Other

        public void DisplayInsufficientResources()
        {
            Alert.ShowAlert("Alert!",
                "You do not have enough resources to purchase this part!", "Okay", null);
        }

        #endregion //Other

        //============================================================================================================//

        public void ShowPartDetails(bool show, in ScrapyardPart scrapyardPart)
        {
            partDetailsContainerRectTransform.gameObject.SetActive(show);

            if (!show)
                return;

            var canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;
            //TODO Get the world position of the object, convert to canvas space
            var screenPoint =
                CameraController.Camera.WorldToScreenPoint(scrapyardPart.transform.position + Vector3.right);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null,
                out var localPoint);

            partDetailsContainerRectTransform.anchoredPosition = localPoint;

            //====================================================================================================================//


            var partData = scrapyardPart.ToBlockData();

            //====================================================================================================================//

            var partType = (PART_TYPE) partData.Type;

            var partRemote = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType);
            var bitType = !partRemote.partGrade.Types.IsNullOrEmpty() ? partRemote.partGrade.Types[0] : BIT_TYPE.NONE;

            var partProfile = FactoryManager.Instance.PartsProfileData.GetProfile(partType);
            var bitProfile = bitType == BIT_TYPE.NONE
                ? new BitProfile()
                : FactoryManager.Instance.BitProfileData.GetProfile(bitType);

            var patchRemoteData = FactoryManager.Instance.PatchRemoteData;

            //====================================================================================================================//

            partNameText.text = partRemote.name;
            partImage.sprite = partProfile.Sprite;

            otherPartDetailsText.text = GetAltDetails(partData, partRemote);

            for (var i = 0; i < partData.Patches.Length; i++)
            {
                var patchData = partData.Patches[i];
                var type = (PATCH_TYPE) patchData.Type;

                patchUis[i].backgroundImage.enabled = type != PATCH_TYPE.EMPTY;

                patchUis[i].text.text = type == PATCH_TYPE.EMPTY
                    ? string.Empty
                    : $"{patchRemoteData.GetRemoteData(type).name} {patchData.Level + 1}";

            }

            //====================================================================================================================//

            partDetailsText.text = $"{GetPartDetails(partType)}";


            //====================================================================================================================//

            for (var i = 0; i < 5; i++)
            {
                var hasSprite = bitType != BIT_TYPE.NONE;

                gradeUis[i].bitImage.enabled = hasSprite;

                if (hasSprite)
                {
                    gradeUis[i].bitImage.sprite = bitProfile.GetSprite(i);

                    var hasLevel = partRemote.partGrade.minBitLevel <= i;
                    gradeUis[i].bitImage.color = hasLevel ? Color.white : Color.grey;
                    gradeUis[i].bitImage.rectTransform.localScale = hasLevel ? Vector3.one : Vector3.one * 0.9f;
                }

                gradeUis[i].text.text = $"{GetGradeDetails(i, partRemote)}";
            }

            //====================================================================================================================//

        }

        //====================================================================================================================//

        private string GetPartDetails(in PART_TYPE partType)
        {
            //var partRemote = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partType);

            switch (partType)
            {
                case PART_TYPE.CORE:
                    return "Core Magnetism";
                case PART_TYPE.REPAIR:
                    return "Repair bit hp per second";
                case PART_TYPE.ARMOR:
                    return "Absorb Bot Damage";
                case PART_TYPE.GUN:
                    return "Bullet Damage";
                case PART_TYPE.SHIELD:
                    return "Invulnerable for x Seconds";
                case PART_TYPE.FREEZE:
                    return "Stun Enemies in radius for x Seconds";
                case PART_TYPE.BOMB:
                    return "Damage in Radius";
                case PART_TYPE.SNIPER:
                    return "Bullet Damage";
                case PART_TYPE.VAMPIRE:
                    return "heals % of damage dealt for 2 seconds";
                case PART_TYPE.UPGRADER:
                    return "Increases grade level of adjacent parts";
                case PART_TYPE.WILDCARD:
                    return
                        "Completes combos with 2 bits\n(Cannot combine with other wild cards, only works as end pieces)";
                case PART_TYPE.RAILGUN:
                    return "vertical shooter weapon";
                default:
                    throw new ArgumentOutOfRangeException(nameof(partType), partType, null);
            }
        }

        private string GetGradeDetails(in int level, in PartRemoteData partRemoteData)
        {
            if (!partRemoteData.HasPartGrade(level, out var value))
                return string.Empty;

            var partRemote = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partRemoteData.partType);
            partRemote.TryGetValue(PartProperties.KEYS.Damage, out float damage);



            switch (partRemoteData.partType)
            {
                case PART_TYPE.CORE:
                    return $"{(int) value}";
                case PART_TYPE.REPAIR:
                    return $"{(int) value}hp/s";
                case PART_TYPE.ARMOR:
                    return $"{(int) (value * 100f)}%";
                case PART_TYPE.GUN:
                    return $"{Mathf.RoundToInt(damage * value)}\ndmg";
                case PART_TYPE.SHIELD:
                    return $"{(int) value}s";
                case PART_TYPE.FREEZE:
                    return $"{(int) value}s";
                case PART_TYPE.BOMB:
                    return $"{Mathf.RoundToInt(damage * value)}\ndmg";
                case PART_TYPE.SNIPER:
                    return $"{Mathf.RoundToInt(damage * value)}\ndmg";
                case PART_TYPE.VAMPIRE:
                    return $"{(int) (value * 100f)}%";
                case PART_TYPE.WILDCARD:
                    return $"{(int) value}";
                case PART_TYPE.RAILGUN:
                    return $"{value:0.0#}s";
                default:
                    throw new ArgumentOutOfRangeException(nameof(partRemoteData.partType), partRemoteData.partType,
                        null);
            }
        }

        private string GetAltDetails(in PartData partData, in PartRemoteData partRemoteData)
        {
            float GetPatchMultiplier(in PatchData[] patches, PATCH_TYPE patchType)
            {

                var outValue = 1f;
                var patchDatas = patches.Where(x => x.Type == (int) patchType).ToArray();

                if (patchDatas.IsNullOrEmpty())
                    return outValue;

                var patchRemoteData = FactoryManager.Instance.PatchRemoteData;


                foreach (var patchData in patchDatas)
                {
                    if (!patchRemoteData.GetRemoteData(patchData.Type).TryGetValue(patchData.Level,
                        PartProperties.KEYS.Multiplier, out float multiplier))
                        continue;

                    if (patchType == PATCH_TYPE.FIRE_RATE)
                        outValue -= multiplier;
                    else
                        outValue += multiplier;
                }

                return outValue;
            }

            var partRemote = FactoryManager.Instance.PartsRemoteData.GetRemoteData(partRemoteData.partType);
            partRemote.TryGetValue(PartProperties.KEYS.Damage, out float damage);
            partRemote.TryGetValue(PartProperties.KEYS.Cooldown, out float cooldown);
            partRemote.TryGetValue(PartProperties.KEYS.Radius, out float range);
            partRemote.TryGetValue(PartProperties.KEYS.Projectile, out string projectileID);


            switch (partRemoteData.partType)
            {
                case PART_TYPE.VAMPIRE:
                case PART_TYPE.SHIELD:
                case PART_TYPE.ARMOR:
                case PART_TYPE.REPAIR:
                case PART_TYPE.CORE:
                case PART_TYPE.UPGRADER:
                case PART_TYPE.WILDCARD:
                    return string.Empty;
                case PART_TYPE.GUN:
                {
                    var projectileRange = FactoryManager.Instance.ProjectileProfile
                        .GetProjectileProfileData(projectileID).ProjectileRange;

                    var dps = Mathf.RoundToInt(damage * GetPatchMultiplier(partData.Patches, PATCH_TYPE.DAMAGE) /
                                               (cooldown * GetPatchMultiplier(partData.Patches, PATCH_TYPE.FIRE_RATE)));
                    var rng = projectileRange * GetPatchMultiplier(partData.Patches, PATCH_TYPE.RANGE);
                    return $"dps {dps}\nrng {rng}";
                }
                case PART_TYPE.FREEZE:
                {
                    var rng = range * GetPatchMultiplier(partData.Patches, PATCH_TYPE.RANGE);
                    return $"rng {rng}\n";
                }
                case PART_TYPE.BOMB:
                {
                    var rng = range * GetPatchMultiplier(partData.Patches, PATCH_TYPE.RANGE);
                    return $"rng {rng}\n";
                }
                case PART_TYPE.SNIPER:
                {
                    var dps = Mathf.RoundToInt(damage * GetPatchMultiplier(partData.Patches, PATCH_TYPE.DAMAGE) /
                                               (cooldown * GetPatchMultiplier(partData.Patches, PATCH_TYPE.FIRE_RATE)));
                    return $"dps {dps}\n";
                }
                case PART_TYPE.RAILGUN:
                {
                    var dps = Mathf.RoundToInt(damage * GetPatchMultiplier(partData.Patches, PATCH_TYPE.DAMAGE) /
                                               (cooldown * GetPatchMultiplier(partData.Patches, PATCH_TYPE.FIRE_RATE)));
                    return $"dps {dps}\n";
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(partRemoteData.partType), partRemoteData.partType,
                        null);
            }

        }

    }
}
