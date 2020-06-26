﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
    public class Brick : MonoBehaviour
    {

        const int brickMoneyMultiplier = 10;

        public Vector2Int arrPos;

        public int brickType;
        public int ID;
        public int brickLevel;
        public int brickHP;
        public int[] brickMaxHP;
        public static int brickMaxLevel = 5;
        public static float brickMoveSpeed = 20f;

        //Resource burn rates
        public bool passiveBurn = false;
        public float[] redBurn;
        public float[] blueBurn;
        public float[] greenBurn;
        public float[] yellowBurn;
        public float[] greyBurn;

        private AudioSource source;

        public Sprite[] spriteArr;
        public int[] requiredPower;

        public GameObject parentBot;
        Bot bot;
        Rigidbody2D rb2D;

        public List<GameObject> neighborList = new List<GameObject>();
        public HealthBar healthBar;

        public List<ShieldBrick> activeShields = new List<ShieldBrick>();

        bool _hasResources = true;

        public bool hasResources
        {
            get { return _hasResources; }
            set
            {
                if (_hasResources != value)
                {
                    _hasResources = value;
                    bool fullResourceCheck = hasResources || (IsCore() && bot.HasFuel());
                    GetComponent<SpriteRenderer>().color = (fullResourceCheck && _isPowered) ? Color.white : Color.gray;
                    if (!fullResourceCheck)
                    {
                        GameController.Instance.hud.SetResourcesPopup(true);
                    }
                }
            }
        }

        //Is this brick fully powered by the grid?
        bool _isPowered = false;

        public bool isPowered
        {
            get { return _isPowered; }
            set
            {
                if (_isPowered != value)
                {
                    if (value)
                    {
                        brickHP = Mathf.Min(brickMaxHP[brickLevel], brickHP + healthDiff);
                        healthDiff = 0;

                        Fuel fuelBrick = GetComponent<Fuel>();
                        if (fuelBrick)
                        {
                            fuelBrick.fuelLevel = Mathf.Min(fuelBrick.maxFuelArr[brickLevel],
                                fuelBrick.fuelLevel + fuelBrick.fuelDiff);
                            fuelBrick.fuelDiff = 0;
                        }

                        ShieldBrick shieldBrick = GetComponent<ShieldBrick>();
                        if (shieldBrick)
                        {
                            shieldBrick.shieldHp = Mathf.Min(shieldBrick.healthAtLevel[brickLevel],
                                shieldBrick.shieldHp + shieldBrick.shieldHealthDiff);
                            shieldBrick.shieldHealthDiff = 0;
                        }
                    }
                    else
                    {
                        healthDiff = Mathf.Max(0, brickHP - brickMaxHP[0]);
                        brickHP = Mathf.Min(brickHP, brickMaxHP[0]);

                        Fuel fuelBrick = GetComponent<Fuel>();
                        if (fuelBrick)
                        {
                            fuelBrick.fuelDiff = Mathf.Max(0, fuelBrick.fuelLevel - fuelBrick.maxFuelArr[0]);
                            fuelBrick.fuelLevel = Mathf.Min(fuelBrick.fuelLevel, fuelBrick.maxFuelArr[0]);
                        }

                        ShieldBrick shieldBrick = GetComponent<ShieldBrick>();
                        if (shieldBrick)
                        {
                            shieldBrick.shieldHealthDiff =
                                Mathf.Max(0, shieldBrick.shieldHp - shieldBrick.healthAtLevel[0]);
                            shieldBrick.shieldHp = Mathf.Min(shieldBrick.shieldHp, shieldBrick.healthAtLevel[0]);
                        }
                    }
                }

                _isPowered = value;
                bool fullResourceCheck = hasResources || (IsCore() && bot.HasFuel());
                GetComponent<SpriteRenderer>().color = (fullResourceCheck && _isPowered) ? Color.white : Color.gray;
            }
        }

        //Store change in brick's health if power level changes
        int healthDiff = 0;

        //Return adjusted brick level based on available power
        public int GetPoweredLevel()
        {
            return isPowered ? brickLevel : 0;
        }

        void Awake()
        {
            brickLevel = 0;
            source = GetComponent<AudioSource>();
            rb2D = gameObject.GetComponent<Rigidbody2D>();
            healthBar = GetComponentInChildren<HealthBar>();
            _isPowered = false;
            isPowered = true;
            if (!IsParasite())
            {
                brickHP = brickMaxHP[brickLevel];
                healthBar.gameObject.SetActive(false);
            }
        }

        void Start()
        {
            bot = parentBot.GetComponent<Bot>();
            FixedJoint2D fj = gameObject.GetComponent<FixedJoint2D>();
            fj.connectedBody = parentBot.GetComponent<Rigidbody2D>();
            InvokeRepeating("CheckHP", 0.5f, 0.1f);

            bool burnsResource = false;
            foreach (float Resource in redBurn)
            {
                if (Resource > 0)
                {
                    burnsResource = true;
                    break;
                }
            }

            foreach (float Resource in blueBurn)
            {
                if (Resource > 0)
                {
                    burnsResource = true;
                    break;
                }
            }

            foreach (float Resource in greenBurn)
            {
                if (Resource > 0)
                {
                    burnsResource = true;
                    break;
                }
            }

            foreach (float Resource in yellowBurn)
            {
                if (Resource > 0)
                {
                    burnsResource = true;
                    break;
                }
            }

            foreach (float Resource in greyBurn)
            {
                if (Resource > 0)
                {
                    burnsResource = true;
                    break;
                }
            }

            if (burnsResource)
            {
                bot.resourceBurnBricks.Add(this);
            }
        }

        //If resources burn constantly, apply here
        void Update()
        {
            if (passiveBurn)
            {
                TryBurnResources(Time.deltaTime);
            }
        }

        private void OnEnable()
        {
            GameController.OnLevelComplete += OnLevelComplete;
        }

        private void OnDisable()
        {
            GameController.OnLevelComplete -= OnLevelComplete;
        }

        void OnLevelComplete()
        {
            if (IsParasite())
            {
                MakeOrphan();
            }
        }

        //Burn corresponding resource amounts, if able
        public bool TryBurnResources(float interval)
        {
            if (GameController.Instance.isLevelCompleteQueued)
                return true;

            int burnLevel = GetPoweredLevel();
            if (bot.storedRed >= interval * redBurn[burnLevel] && bot.storedBlue >= interval * blueBurn[burnLevel] &&
                bot.storedGreen >= interval * greenBurn[burnLevel] &&
                bot.storedYellow >= interval * yellowBurn[burnLevel] &&
                bot.storedGrey >= interval * greyBurn[burnLevel])
            {
                bot.storedRed -= interval * redBurn[burnLevel];
                bot.storedBlue -= interval * blueBurn[burnLevel];
                bot.storedGreen -= interval * greenBurn[burnLevel];
                bot.storedYellow -= interval * yellowBurn[burnLevel];
                bot.storedGrey -= interval * greyBurn[burnLevel];
                hasResources = true;
                return true;
            }

            hasResources = false;
            return false;
        }

        void CheckHP()
        {
            if (brickHP <= 0)
                ExplodeBrick();
        }

        public bool IsParasite()
        {
            if (GetComponent<Parasite>() == null)
                return false;
            else
                return true;
        }

        public bool IsCrafted()
        {
            return GetComponent<CraftedPart>() != null;
        }

        public void AdjustHP(int damage)
        {

            if (IsParasite())
            {
                GetComponent<Parasite>().AdjustHP(damage);
            }
            else if (damage > 0 || activeShields.Count == 0)
            {
                brickHP += damage;
            }
            else
            {
                ShieldBrick blockingShield = null;
                float blockedPercent = 0;
                foreach (ShieldBrick shield in activeShields)
                {
                    if (shield && (!blockingShield ||
                                   shield.percentBlockedAtLevel[shield.GetComponent<Brick>().GetPoweredLevel()] >
                                   blockedPercent))
                    {
                        blockingShield = shield;
                        blockedPercent = shield.percentBlockedAtLevel[shield.GetComponent<Brick>().GetPoweredLevel()];
                    }
                }

                if (blockingShield)
                {
                    int damageBlocked = Mathf.RoundToInt(blockedPercent * damage);
                    brickHP += damage - damageBlocked;
                    blockingShield.shieldHp += damageBlocked;
                }
                else
                {
                    brickHP += damage;
                }
            }

            if (!IsParasite() && brickHP > 0)
            {
                if (brickHP >= brickMaxHP[GetPoweredLevel()])
                {
                    brickHP = brickMaxHP[GetPoweredLevel()];
                    healthBar.gameObject.SetActive(false);
                }
                else if (!healthBar.gameObject.activeSelf)
                {
                    healthBar.gameObject.SetActive(true);
                    float normalizedHealth = (float) brickHP / (float) brickMaxHP[GetPoweredLevel()];
                    healthBar.SetSize(normalizedHealth);
                }
                else
                {
                    float normalizedHealth = (float) brickHP / (float) brickMaxHP[GetPoweredLevel()];
                    healthBar.SetSize(normalizedHealth);
                }
            }
        }

        public int BitBrickCollide(GameObject bitObj)
        {
            Transform t = bitObj.transform.parent;
            if (t == null)
                return 0;

            GameObject blockObj = t.gameObject;
            Block block = blockObj.GetComponent<Block>();
            Bit bit = bitObj.GetComponent<Bit>();
            if (bit == null)
                return 0;

            int bitType = bit.bitType;
            float rA = parentBot.transform.rotation.eulerAngles.z;

            if (blockObj == null)
                return 0;

            if (bit == null)
                return 0;

            if (bitType == 0) // black bit - hurt the brick
            {
                AdjustHP(-1000);
                if (!IsParasite())
                {
                    bot.GetComponent<Overheat>().AddHeat();
                }

                bit.RemoveFromBlock("explode");

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnAsteroidHit();
                }


            }
            else
            {
                if (!((rA == 0) || (rA == 90) || (rA == 180) || (rA == 270)))
                {
                    block.BounceBlock();
                }
                else
                {
                    Vector2Int bitCoords = ScreenStuff.GetOffset(bitObj);
                    Vector2Int brickCoords = ScreenStuff.GetOffset(gameObject);
                    Vector2Int hitDirV2 = brickCoords - bitCoords;

                    if (hitDirV2 == new Vector2Int(0, 0))
                    {
                        block.BounceBlock();
                    }

                    if (bitType == 1) // white bit - bump the brick
                    {
                        bot.BumpColumn(arrPos, hitDirV2);
                        block.BounceBlock();
                    }
                    else
                    {
                        bot.ResolveCollision(blockObj, hitDirV2);
                    }
                }
            }

            return 1;
        }

        public Vector2Int ScreenArrPos()
        {
            return bot.BotToScreenCoords(arrPos);
        }


        public void RotateUpright()
        {
            transform.rotation = Quaternion.identity;
        }

        public bool BitIsAboveBrick(Collision2D col)
        {
            if (col.transform.position.y >= (gameObject.transform.position.y + ScreenStuff.rowSize - 0.8))
                return true;
            else
                return false;
        }

        bool isExploding = false;

        public void ExplodeBrick()
        {
            if (isExploding)
                return;
            isExploding = true;

            Animator anim;
            float animDuration;

            if (!IsParasite())
                bot.queueDestroyedBrick = true;

            if (brickType == 9 && (bot.BrickAtBotArr(bot.coreV2) == null))
            {

                GameController.Instance.EndGame("CORE DESTROYED");

            }

            if (GetComponent<ShieldBrick>())
            {
                GetComponent<ShieldBrick>().ToggleShield(false);
            }

            if (GetComponent<Bomb>())
            {
                GetComponent<Bomb>().BombEnemies(GetPoweredLevel());
            }

            if ((brickType == 1) && (GetComponent<Fuel>().fuelLevel > 0))
            {
                for (int x = 0; x < neighborList.Count; x++)
                {
                    if (!neighborList[x] || !neighborList[x].GetComponent<Brick>())
                    {
                        neighborList.RemoveAt(x--);
                    }
                    else if (neighborList[x].GetComponent<Brick>().brickType == 1)
                    {
                        neighborList[x].GetComponent<Brick>().AdjustHP(-10);
                    }
                }
            }

            anim = gameObject.GetComponent<Animator>();
            anim.enabled = true;
            animDuration = 0.3f;

            RemoveBrickFromBotArray();
            StartCoroutine(DestroyAfterAnimation(animDuration));
        }

        IEnumerator DestroyAfterAnimation(float duration)
        {
            yield return new WaitForSeconds(duration);
            Destroy(gameObject);
        }

        public void DestroyBrick()
        {
            RemoveBrickFromBotArray();
            Destroy(gameObject);
        }

        public void MakeOrphan()
        {

            RemoveBrickFromBotArray();
            rb2D.isKinematic = false;

            transform.parent = null;
            tag = "Moveable";

            GetComponent<BoxCollider2D>().enabled = false;
            rb2D.gravityScale = 4;
        }

        public void RemoveBrickFromBotArray()
        {
            bot = parentBot.GetComponent<Bot>();
            bot.SetBrickAtBotArr(arrPos, null);
            bot.brickTypeArr[arrPos.x, arrPos.y] = -1;
            bot.brickList.Remove(gameObject);
            //if (IsParasite())
            //    GameController.Instance.enemyList.Remove(gameObject);

            for (int i = 0; i < activeShields.Count; i++)
            {
                if (activeShields[i])
                {
                    activeShields[i--].GetComponentInChildren<ShieldTrigger>().OnExitShield(this);
                }
                else
                {
                    activeShields.RemoveAt(i--);
                }
            }

            if (bot.BrickAtBotArr(bot.coreV2) == null)
            {

                GameController.Instance.EndGame("CORE DESTROYED");
            }

            if (GetComponent<Container>())
                bot.RemoveContainer(GetComponent<Container>());

            if (bot.fuelBrickList.Contains(gameObject))
            {
                GetComponent<Fuel>().CancelBurnFuel();
                bot.fuelBrickList.Remove(gameObject);
            }

            if (bot.resourceBurnBricks.Contains(this))
                bot.resourceBurnBricks.Remove(this);

            bot.RefreshNeighborLists();
            bot.orphanCheckFlag = true;
        }

        public void MoveBrick(Vector2Int newArrPos)
        {

            // move this brick to newArrPos.x, newArrPos.y

            if ((bot.IsValidBrickPos(newArrPos) &&
                 (bot.BrickAtBotArr(newArrPos) == null)))
            {
                // update the array

                bot.SetBrickAtBotArr(newArrPos, gameObject);
                bot.brickTypeArr[newArrPos.x, newArrPos.y] = brickType;

                bot.SetBrickAtBotArr(arrPos, null);
                bot.brickTypeArr[arrPos.x, arrPos.y] = -1;

                // move the gameObject

                SmoothMoveBrickObj(newArrPos);

                arrPos = newArrPos;

                // update neighbor lists

                bot.RefreshNeighborLists();
            }
        }

        public void SmoothMoveBrickObj(Vector2Int newArrPos)
        {
            Vector2Int newOffset = ScreenStuff.ScreenToBotOffset(bot.ArrToOffset(newArrPos), bot.botRotation);
            Vector3 newOffsetV3 = new Vector3(newOffset.x * ScreenStuff.colSize, newOffset.y * ScreenStuff.colSize, 0);

            StartCoroutine(SlideBrickOverTime(rb2D.transform.position, newOffsetV3));
        }


        IEnumerator SlideBrickOverTime(Vector3 originalPos, Vector3 newPos)
        {
            float t = 0f;
            float duration = (newPos - originalPos).magnitude / brickMoveSpeed;

            while (t < duration)
            {
                rb2D.transform.position = Vector3.Lerp(originalPos, newPos, t / duration);
                yield return null;
                t += Time.deltaTime;
            }

            rb2D.transform.position = newPos;
        }


        public bool IsCore()
        {
            if (bot && arrPos == bot.coreV2)
                return true;
            else
                return false;
        }

        public void SetLevel(int level)
        {
            brickLevel = level;
            if (level < spriteArr.Length)
                this.GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
        }

        public void UpgradeBrick()
        {
            if (brickLevel < spriteArr.Length - 1)
            {
                isPowered = true;
                brickLevel++;
                ID++;
                brickHP = brickMaxHP[brickLevel];
                healthBar.gameObject.SetActive(false);
                GetComponent<SpriteRenderer>().sprite = spriteArr[brickLevel];
                if (brickType == 1)
                    gameObject.GetComponent<Fuel>().UpgradeFuelLevel();

                if (GetComponent<Container>())
                {
                    bot.UpdateContainers();
                }

                int scoreIncrease = (int) Mathf.Pow(brickMoneyMultiplier, brickLevel);
                //GameController.Instance.money += scoreIncrease;
                //GameController.Instance.CreateFloatingText("$" + scoreIncrease, transform.position, 40, Color.white);
            }
        }


        public void HealMaxHP()
        {
            brickHP = brickMaxHP[GetPoweredLevel()];
            healthBar.gameObject.SetActive(false);
        }

        public int ConvertToBitType()
        {
            return brickType + 2;
        }

        public int ConvertToEnemyType()
        {
            return brickType - 7;
        }

        public bool CompareToBit(Bit bit)
        {
            int compType = Mathf.RoundToInt((ID - bit.bitLevel) / 1000) - 2;

            return ((compType == brickType) && (bit.bitLevel == brickLevel));
        }

    }
}