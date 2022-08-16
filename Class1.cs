using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using UnityEngine.VFX;

namespace NewMonado
{
    public class MonadoModule : ItemModule
    {
        public bool Voices;
        public float BusterDamage;
        public float EnchantAuraTime;
        public float ShieldAuraTime;
        public float ShieldDamageAbsorb;
        public float SpeedAuraTime;
        public float SpeedMultiplier;
        public float PurgeAuraTime;
        public float CycloneAuraTime;
        public float ArmorAuraTime;
        public float ArmorDamageReductionMult;
        public float EaterAuraTime;
        public float EaterDamageOverTime;
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<MonadoComponent>().Setup(Voices, BusterDamage, EnchantAuraTime, ShieldAuraTime, ShieldDamageAbsorb, SpeedAuraTime, SpeedMultiplier, PurgeAuraTime, CycloneAuraTime, ArmorAuraTime, ArmorDamageReductionMult, EaterAuraTime, EaterDamageOverTime);
        }
    }
    public class MonadoComponent : MonoBehaviour
    {
        Item item;
        Damager slash;
        Damager pierce;
        GameObject center;
        GameObject defaultSymbol;
        GameObject busterSymbol;
        GameObject enchantSymbol;
        GameObject shieldSymbol;
        GameObject speedSymbol;
        GameObject purgeSymbol;
        GameObject cycloneSymbol;
        GameObject armorSymbol;
        GameObject eaterSymbol;
        GameObject beam;
        GameObject beamCross;
        GameObject cycloneFX;
        GameObject ringDefault;
        GameObject ringAbility;
        Animation animator;
        string component;
        Vector3 original;
        bool active = false;
        int clickCounter = 1;
        string voiceText = "";
        bool Voices;
        public float BusterDamage;
        public float EnchantAuraTime;
        public float ShieldAuraTime;
        public float ShieldDamageAbsorb;
        public float SpeedAuraTime;
        public float SpeedMultiplier;
        public float PurgeAuraTime;
        public float CycloneAuraTime;
        public float ArmorAuraTime;
        public float ArmorDamageReductionMult;
        public float EaterAuraTime;
        public float EaterDamageOverTime;
        public void Start()
        {
            item = GetComponent<Item>();
            slash = item.GetCustomReference("Slash").GetComponent<Damager>();
            pierce = item.GetCustomReference("Pierce").GetComponent<Damager>();
            center = item.GetCustomReference("Center").gameObject;
            defaultSymbol = item.GetCustomReference("Default").gameObject;
            busterSymbol = item.GetCustomReference("Buster").gameObject;
            enchantSymbol = item.GetCustomReference("Enchant").gameObject;
            shieldSymbol = item.GetCustomReference("Shield").gameObject;
            speedSymbol = item.GetCustomReference("Speed").gameObject;
            purgeSymbol = item.GetCustomReference("Purge").gameObject;
            cycloneSymbol = item.GetCustomReference("Cyclone").gameObject;
            armorSymbol = item.GetCustomReference("Armor").gameObject;
            eaterSymbol = item.GetCustomReference("Eater").gameObject;
            if (item.data.id != "NewMonadoIII")
            {
                ringDefault = item.GetCustomReference("RingDefault")?.gameObject;
                ringAbility = item.GetCustomReference("RingAbility")?.gameObject;
            }
            beam = item.GetCustomReference("Beam").gameObject;
            beamCross = item.GetCustomReference("BeamCross").gameObject;
            cycloneFX = item.GetCustomReference("CycloneFX").gameObject;
            animator = item.gameObject.GetComponent<Animation>();
            item.OnHeldActionEvent += Item_OnHeldActionEvent;
            item.OnGrabEvent += Item_OnGrabEvent;
            item.OnSnapEvent += Item_OnSnapEvent;
            original = beam.transform.localPosition;
            item.mainCollisionHandler.OnCollisionStartEvent += MainCollisionHandler_OnCollisionStartEvent;
            animator.Play("Deactivate");
            foreach(MaterialData data in Catalog.GetDataList(Catalog.Category.Material))
            {
                if (!slash.data.damageModifierData.collisions[0].targetMaterials.Contains(data)) slash.data.damageModifierData.collisions[0].targetMaterials.Add(data);
                if (!pierce.data.damageModifierData.collisions[0].targetMaterials.Contains(data)) pierce.data.damageModifierData.collisions[0].targetMaterials.Add(data);
            }
        }
        public void Setup(bool voices, float busterDamage, float enchantTime, float shieldTime, float shieldAbsorb, float speedTime, float speedMult, float purgeTime, float cycloneTime, float armorTime, float armorMult, float eaterTime, float eaterDOT)
        {
            Voices = voices;
            BusterDamage = busterDamage;
            EnchantAuraTime = enchantTime;
            ShieldAuraTime = shieldTime;
            ShieldDamageAbsorb = shieldAbsorb;
            SpeedAuraTime = speedTime;
            SpeedMultiplier = speedMult;
            PurgeAuraTime = purgeTime;
            CycloneAuraTime = cycloneTime;
            ArmorAuraTime = armorTime;
            ArmorDamageReductionMult = armorMult;
            EaterAuraTime = eaterTime;
            EaterDamageOverTime = eaterDOT;
            if (Voices) voiceText = "Voice";
        }

        private void Item_OnSnapEvent(Holder holder)
        {
            clickCounter = 1;
            SwapAbility(clickCounter);
            if (active) ToggleBlade();
        }

        private void MainCollisionHandler_OnCollisionStartEvent(CollisionInstance collisionInstance)
        {
            if (collisionInstance.sourceCollider.gameObject == beam || collisionInstance.targetCollider.gameObject == beam)
            {
                if (collisionInstance.targetCollider.GetComponentInParent<Creature>() != null && collisionInstance.targetCollider.GetComponentInParent<Creature>() != Player.local.creature && !collisionInstance.targetCollider.GetComponentInParent<Creature>().isKilled)
                    switch (component)
                    {
                        case "Purge":
                            Destroy(collisionInstance.targetCollider.GetComponentInParent<Creature>().gameObject.GetComponent<PurgeAura>());
                            collisionInstance.targetCollider.GetComponentInParent<Creature>().gameObject.AddComponent<PurgeAura>().Setup(PurgeAuraTime);
                            ActivateAura("MonadoPurge");
                            clickCounter = 1;
                            SwapAbility(clickCounter);
                            break;
                        case "Eater":
                            Destroy(collisionInstance.targetCollider.GetComponentInParent<Creature>().gameObject.GetComponent<EaterAura>());
                            collisionInstance.targetCollider.GetComponentInParent<Creature>().gameObject.AddComponent<EaterAura>().Setup(EaterAuraTime, EaterDamageOverTime);
                            ActivateAura("MonadoEater");
                            clickCounter = 1;
                            SwapAbility(clickCounter);
                            break;
                        default:
                            break;
                    }
                if (component == "Buster" && collisionInstance.sourceCollider.GetComponentInParent<Creature>() != Player.local.creature)
                {
                    if (item.isPenetrating && collisionInstance.sourceColliderGroup.GetComponentInParent<Creature>() != null) StartCoroutine(SliceAll(collisionInstance.sourceColliderGroup.GetComponentInParent<Creature>()));
                    StartCoroutine(Buster(collisionInstance.contactPoint, collisionInstance.contactNormal, collisionInstance.sourceColliderGroup.transform.up));
                    ActivateAura("MonadoBuster");
                    clickCounter = 1;
                    SwapAbility(clickCounter);
                }
            }
        }
        private IEnumerator SliceAll(Creature creature)
        {
            foreach(RagdollPart part in creature.ragdoll.parts)
            {
                if (part.sliceAllowed)
                {
                    yield return null;
                    part.ragdoll.TrySlice(part);
                }
            }
        }
        private IEnumerator Buster(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward)
        {
            EffectInstance effectInstance = Catalog.GetData<EffectData>("SpellGravityShockwave").Spawn(contactPoint, Quaternion.LookRotation(-contactNormal, contactNormalUpward));
            effectInstance.Play();
            effectInstance.SetIntensity(15); 
            Collider[] sphereContacts = Physics.OverlapSphere(contactPoint, 15, 218119169);
            List<Creature> creaturesPushed = new List<Creature>();
            List<Rigidbody> rigidbodiesPushed = new List<Rigidbody>();
            rigidbodiesPushed.Add(item.rb); 
            if (item.lastHandler?.creature)
                creaturesPushed.Add(item.lastHandler.creature);
            float waveDistance = 0.0f;
            while (waveDistance < 15)
            {
                waveDistance += 20f * 0.05f;
                foreach (Collider collider in sphereContacts)
                {
                    if (collider.attachedRigidbody && !collider.attachedRigidbody.isKinematic && Vector3.Distance(contactPoint, collider.transform.position) < waveDistance)
                    {
                        if (collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.NPC) || collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.Ragdoll))
                        {
                            RagdollPart component = collider.attachedRigidbody.gameObject.GetComponent<RagdollPart>();
                            if (component && !creaturesPushed.Contains(component.ragdoll.creature))
                            {
                                if (item?.lastHandler?.creature)
                                {
                                    component.ragdoll.creature.lastInteractionTime = Time.time;
                                    component.ragdoll.creature.lastInteractionCreature = item.lastHandler.creature;
                                }
                                CollisionInstance collision = new CollisionInstance(new DamageStruct(DamageType.Energy, BusterDamage));
                                collision.damageStruct.hitRagdollPart = component;
                                component.ragdoll.creature.Damage(collision);
                                component.ragdoll.creature.TryPush(Creature.PushType.Parry, (component.ragdoll.rootPart.transform.position - contactPoint).normalized, 2);
                                creaturesPushed.Add(component.ragdoll.creature);
                            }
                        }
                        if (collider.attachedRigidbody.gameObject.layer != GameManager.GetLayer(LayerName.NPC) && !rigidbodiesPushed.Contains(collider.attachedRigidbody))
                        {
                            collider.attachedRigidbody.AddExplosionForce(20, contactPoint, 15, 0.5f, ForceMode.VelocityChange);
                            rigidbodiesPushed.Add(collider.attachedRigidbody);
                        }
                    }
                }
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void Item_OnGrabEvent(Handle handle, RagdollHand ragdollHand)
        {
            if (ragdollHand.creature.data.id == "Shulk")
            {
                clickCounter = 1;
                SwapAbility(clickCounter);
                if (!active) ToggleBlade();
            }
            else if (ragdollHand.creature != Player.local.creature)
            {
                if (!active) ToggleBlade();
                Creature target = ragdollHand.GetComponentInParent<Creature>();
                target.TryElectrocute(1, 3, true, false, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
                ragdollHand.UnGrab(false);
            }
        }

        private void Item_OnHeldActionEvent(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (((action == Interactable.Action.AlternateUseStart) && PlayerControl.GetHand(ragdollHand.side).castPressed) ||
                (action == Interactable.Action.UseStart && PlayerControl.GetHand(ragdollHand.side).alternateUsePressed))
            {
                PlayerControl.GetHand(ragdollHand.side).HapticPlayClip(Catalog.gameData.haptics.spellSelected);
                ToggleBlade();
                clickCounter = 1;
                SwapAbility(clickCounter);
                if (!active && item.isPenetrating)
                {
                    pierce.UnPenetrateAll();
                    slash.UnPenetrateAll();
                }
                if (!active)
                {
                    ringAbility?.SetActive(false);
                    ringDefault?.SetActive(true);
                }
            }
            if (active && action == Interactable.Action.AlternateUseStart && !PlayerControl.GetHand(ragdollHand.side).castPressed)
            {
                clickCounter++;
                SwapAbility(clickCounter);
            }
        }
        public void ToggleBlade()
        {
            if (!active)
            {
                animator.Play("Activate");
                if (item.data.id != "NewMonadoIII")
                {
                    foreach(ColliderGroup group in item.colliderGroups)
                    {
                        if (group.name.Equals("Front") || group.name.Equals("Back"))
                            group.imbueEffectRenderer.gameObject.AddComponent<ShockEffect>();
                    }
                }
            }
            else if (active)
            {
                animator.Play("Deactivate"); 
                if (item.data.id != "NewMonadoIII")
                {
                    foreach (ColliderGroup group in item.colliderGroups)
                    {
                        if (group.name.Equals("Front") || group.name.Equals("Back"))
                            Destroy(group.imbueEffectRenderer.gameObject.GetComponent<ShockEffect>());
                    }
                }
            }
            active = !active;
        }
        public class ShockEffect : MonoBehaviour
        {
            EffectInstance instance;
            public void Start()
            {
                instance = Catalog.GetData<EffectData>("MonadoElectricity").Spawn(gameObject.transform);
                instance.SetRenderer(gameObject.GetComponent<Renderer>(), false);
                instance.SetIntensity(0.1f);
                instance.Play();
            }
            public void OnDestroy()
            {
                instance.Stop();
            }
        }
        public void SetDefault()
        {
            Color defaultBlade = new Color(1, 1, 1, 1);
            Color defaultBladeglow = new Color(48, 189, 191, 0);
            Color defaultCenter = new Color(150, 255, 255, 42);
            Color defaultCenterglow = new Color(16, 191, 191, 0);
            Color defaultRing = new Color(8, 225, 231, 177);
            Color defaultRingglow = new Color(1, 191, 191, 0);
            SetColor(defaultBlade, defaultBladeglow, defaultCenter, defaultCenterglow, defaultRing, defaultRingglow, defaultSymbol);
            beam.transform.localScale = new Vector3(1, 7, 1);
            beam.transform.localPosition = original;
            //slash.UnPenetrateAll();
            //pierce.UnPenetrateAll();
            component = null;
            ringAbility?.SetActive(false);
            ringDefault?.SetActive(true);
            cycloneFX.SetActive(false);
        }
        public void SwapAbility(int counter)
        {
            switch (counter.ToString())
            {
                case "1":
                    SetDefault();
                    break;
                case "2":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    beam.transform.localScale = new Vector3(1, 14, 1);
                    beam.transform.localPosition = new Vector3(original.x, original.y + 0.7f, original.z);
                    Color busterBlade = new Color(2, 2, 2, 1);
                    Color busterBladeglow = new Color(54, 186, 191, 0);
                    Color busterCenter = new Color(54, 186, 191, 42);
                    Color busterCenterglow = new Color(54, 186, 191, 0);
                    Color busterRing = new Color(54, 186, 191, 177);
                    Color busterRingglow = new Color(54, 186, 191, 0);
                    SetColor(busterBlade, busterBladeglow, busterCenter, busterCenterglow, busterRing, busterRingglow, busterSymbol);
                    component = "Buster";
                    break;
                case "3":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color enchantBlade = new Color(0.992156863f, 0.0431372549f, 0.53333333333f, 1);
                    Color enchantBladeglow = new Color(119, 33, 191, 0);
                    Color enchantCenter = new Color(197, 109, 243, 42);
                    Color enchantCenterglow = new Color(119, 33, 191, 0);
                    Color enchantRing = new Color(197, 109, 243, 177);
                    Color enchantRingglow = new Color(119, 33, 191, 0);
                    SetColor(enchantBlade, enchantBladeglow, enchantCenter, enchantCenterglow, enchantRing, enchantRingglow, enchantSymbol);
                    component = "Enchant";
                    break;
                case "4":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color shieldBlade = new Color(1, 0.164705882f, 0, 1);
                    Color shieldBladeglow = new Color(191, 91, 0, 0);
                    Color shieldCenter = new Color(255, 183, 0, 42);
                    Color shieldCenterglow = new Color(191, 137, 0, 0);
                    Color shieldRing = new Color(255, 183, 0, 177);
                    Color shieldRingglow = new Color(191, 91, 0, 0);
                    SetColor(shieldBlade, shieldBladeglow, shieldCenter, shieldCenterglow, shieldRing, shieldRingglow, shieldSymbol);
                    component = "Shield";
                    break;
                case "5":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color speedBlade = new Color(0, 0.207843137f, 1, 1);
                    Color speedBladeglow = new Color(0, 79, 191, 0);
                    Color speedCenter = new Color(0, 172, 255, 42);
                    Color speedCenterglow = new Color(0, 79, 191, 0);
                    Color speedRing = new Color(0, 172, 255, 177);
                    Color speedRingglow = new Color(0, 79, 191, 0);
                    SetColor(speedBlade, speedBladeglow, speedCenter, speedCenterglow, speedRing, speedRingglow, speedSymbol);
                    component = "Speed";
                    break;
                case "6":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color purgeBlade = new Color(0.0431372549f, 1, 0.2f, 1);
                    Color purgeBladeglow = new Color(39, 191, 80, 0);
                    Color purgeCenter = new Color(40, 195, 114, 42);
                    Color purgeCenterglow = new Color(39, 191, 112, 0);
                    Color purgeRing = new Color(40, 195, 114, 177);
                    Color purgeRingglow = new Color(39, 191, 112, 0);
                    SetColor(purgeBlade, purgeBladeglow, purgeCenter, purgeCenterglow, purgeRing, purgeRingglow, purgeSymbol);
                    component = "Purge";
                    break;
                case "7":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color cycloneBlade = new Color(1, 1, 1, 1);
                    Color cycloneBladeglow = new Color(191, 191, 191, 0);
                    Color cycloneCenter = new Color(255, 255, 255, 42);
                    Color cycloneCenterglow = new Color(191, 191, 191, 0);
                    Color cycloneRing = new Color(255, 255, 255, 177);
                    Color cycloneRingglow = new Color(191, 191, 191, 0);
                    SetColor(cycloneBlade, cycloneBladeglow, cycloneCenter, cycloneCenterglow, cycloneRing, cycloneRingglow, cycloneSymbol);
                    cycloneFX.SetActive(true);
                    component = "Cyclone";
                    break;
                case "8":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color armorBlade = new Color(1, 0.164705882f, 0, 1);
                    Color armorBladeglow = new Color(191, 91, 0, 0);
                    Color armorCenter = new Color(255, 183, 0, 42);
                    Color armorCenterglow = new Color(191, 137, 0, 0);
                    Color armorRing = new Color(255, 183, 0, 177);
                    Color armorRingglow = new Color(191, 91, 0, 0);
                    SetColor(armorBlade, armorBladeglow, armorCenter, armorCenterglow, armorRing, armorRingglow, armorSymbol);
                    component = "Armor";
                    break;
                case "9":
                    SetDefault();
                    ringAbility?.SetActive(true);
                    ringDefault?.SetActive(false);
                    Color eaterBlade = new Color(0.0470588235f, 0, 1, 1);
                    Color eaterBladeglow = new Color(61, 18, 191, 0);
                    Color eaterCenter = new Color(47, 20, 82, 42);
                    Color eaterCenterglow = new Color(40, 11, 124, 0);
                    Color eaterRing = new Color(47, 20, 82, 177);
                    Color eaterRingglow = new Color(61, 18, 191, 0);
                    SetColor(eaterBlade, eaterBladeglow, eaterCenter, eaterCenterglow, eaterRing, eaterRingglow, eaterSymbol);
                    component = "Eater";
                    clickCounter = 0;
                    break;
            }
        }
        public void FixedUpdate()
        {
            if (item.rb.velocity.magnitude - Player.local.locomotion.rb.velocity.magnitude > 15f)
            {
                switch (component)
                {
                    case "Enchant":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.mainHandler.creature.factionId && creature != item.mainHandler.creature && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<EnchantAura>());
                                creature.gameObject.AddComponent<EnchantAura>().Setup(EnchantAuraTime);
                            }
                        }
                        ActivateAura("MonadoEnchant");
                        clickCounter = 1;
                        SwapAbility(clickCounter);
                        break;
                    case "Shield":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.mainHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<ShieldAura>());
                                creature.gameObject.AddComponent<ShieldAura>().Setup(ShieldAuraTime, ShieldDamageAbsorb);
                            }
                        }
                        ActivateAura("MonadoShield");
                        clickCounter = 1;
                        SwapAbility(clickCounter);
                        break;
                    case "Speed":
                        ActivateAura("MonadoSpeed");
                        Destroy(Player.local.gameObject.GetComponent<SpeedAura>());
                        Player.local.gameObject.AddComponent<SpeedAura>().Setup(SpeedAuraTime, SpeedMultiplier);
                        clickCounter = 1;
                        SwapAbility(clickCounter);
                        break;
                    case "Cyclone":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId != item.mainHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<CycloneAura>());
                                creature.gameObject.AddComponent<CycloneAura>().Setup(CycloneAuraTime);
                            }
                        }
                        ActivateAura("MonadoCyclone");
                        clickCounter = 1;
                        SwapAbility(clickCounter);
                        break;
                    case "Armor":
                        foreach (Creature creature in Creature.allActive)
                        {
                            if (creature.factionId == item.mainHandler.creature.factionId && creature.isActiveAndEnabled && !creature.isKilled)
                            {
                                Destroy(creature.gameObject.GetComponent<ArmorAura>());
                                creature.gameObject.AddComponent<ArmorAura>().Setup(ArmorAuraTime, ArmorDamageReductionMult);
                            }
                        }
                        ActivateAura("MonadoArmor");
                        clickCounter = 1;
                        SwapAbility(clickCounter);
                        break;
                    default:
                        break;
                }
            }
        }
        private void SetColor(Color blade, Color bladeEmission, Color centerColor, Color centerEmission, Color ring, Color ringEmission, GameObject symbol)
        {
            beam.GetComponent<VisualEffect>().SetVector4("BeamColor", new Vector4(blade.r * 2, blade.g * 2, blade.b * 2, blade.a));
            beamCross.GetComponent<VisualEffect>().SetVector4("BeamColor", new Vector4(blade.r * 2, blade.g * 2, blade.b * 2, blade.a));
            center.transform.Find("Area Light 0").GetComponent<Light>().color = bladeEmission * 0.01f;
            center.transform.Find("Area Light 1").GetComponent<Light>().color = bladeEmission * 0.01f;
            center.GetComponent<MeshRenderer>().material.SetColor("_Color", centerColor);
            center.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", centerEmission * 0.005f);
            ringAbility?.GetComponent<SkinnedMeshRenderer>().material.SetColor("_Color", ring);
            ringAbility?.GetComponent<SkinnedMeshRenderer>().material.SetColor("_EmissionColor", ringEmission * 0.03f);
            defaultSymbol.SetActive(false);
            busterSymbol.SetActive(false);
            enchantSymbol.SetActive(false);
            shieldSymbol.SetActive(false);
            speedSymbol.SetActive(false);
            purgeSymbol.SetActive(false);
            cycloneSymbol.SetActive(false);
            armorSymbol.SetActive(false);
            eaterSymbol.SetActive(false);
            symbol.SetActive(true);
        }
        public void ActivateAura(string effectId)
        {
            EffectInstance effectInstance = Catalog.GetData<EffectData>(effectId + voiceText).Spawn(item.mainHandler.creature.transform.position, Quaternion.LookRotation(item.mainHandler.creature.transform.up), item.mainHandler.creature.transform);
            effectInstance.SetIntensity(0.1f);
            effectInstance.Play();
        }
        public class EnchantAura : MonoBehaviour
        {
            Creature creature;
            Item rightItem;
            Item leftItem;
            SpellCastCharge imbueSpell;
            float timer;
            EffectInstance instance;
            float auraTime;
            public void Setup(float time)
            {
                auraTime = time;
            }
            public void Start()
            {
                creature = GetComponent<Creature>();
                imbueSpell = Catalog.GetData<SpellCastCharge>("Fire");
                timer = Time.time;
                instance = Catalog.GetData<EffectData>("MonadoEnchantAura").Spawn(creature.transform);
                instance.SetRenderer(creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || creature.isKilled)
                {
                    instance.Stop();
                    Destroy(this);
                }
                else
                {
                    if (creature.handRight.grabbedHandle?.item != null)
                    {
                        rightItem = creature.handRight.grabbedHandle.item;
                        foreach(Imbue imbue in rightItem.imbues)
                        {
                            imbue.Transfer(imbueSpell, 5f);
                        }
                    }
                    if (creature.handLeft.grabbedHandle?.item != null)
                    {
                        leftItem = creature.handLeft.grabbedHandle.item;
                        foreach (Imbue imbue in leftItem.imbues)
                        {
                            imbue.Transfer(imbueSpell, 5f);
                        }
                    }
                }
            }
        }
        public class ShieldAura : MonoBehaviour
        {
            Creature creature;
            float timer;
            EffectInstance instance;
            float auraTime;
            float absorb;
            public void Setup(float time, float damageAbsorb)
            {
                auraTime = time;
                absorb = damageAbsorb;
            }
            public void Start()
            {
                creature = GetComponent<Creature>();
                creature.OnDamageEvent += Creature_OnDamageEvent;
                timer = Time.time;
                instance = Catalog.GetData<EffectData>("MonadoShieldAura").Spawn(creature.transform);
                instance.SetRenderer(creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || creature.isKilled || absorb <= 0)
                {
                    Destroy(this);
                }
            }
            private void Creature_OnDamageEvent(CollisionInstance collisionInstance)
            {
                absorb -= collisionInstance.damageStruct.damage;
                collisionInstance.ignoreDamage = true;
            }
            public void OnDestroy()
            {
                creature.OnDamageEvent -= Creature_OnDamageEvent;
                instance.Stop();
            }
        }
        public class SpeedAura : MonoBehaviour
        {
            Player player;
            float timer;
            public static float originalAirSpeed;
            public static float originalForwardSpeed;
            public static float originalHorizontalSpeed;
            public static float originalVerticalSpeed;
            public static float originalRunSpeed;
            public static float originalStrafeSpeed;
            public static float originalBackwardSpeed;
            public static bool stored = false;
            EffectInstance instance;
            float auraTime;
            float mult;
            public void Setup(float time, float multiplier)
            {
                auraTime = time;
                mult = multiplier;
            }
            public void Start()
            {
                player = GetComponent<Player>();
                timer = Time.time;
                if (!stored)
                {
                    originalAirSpeed = player.locomotion.airSpeed;
                    originalForwardSpeed = player.locomotion.forwardSpeed;
                    originalHorizontalSpeed = player.locomotion.horizontalSpeed;
                    originalVerticalSpeed = player.locomotion.verticalSpeed;
                    originalRunSpeed = player.locomotion.runSpeedAdd;
                    originalStrafeSpeed = player.locomotion.strafeSpeed;
                    originalBackwardSpeed = player.locomotion.backwardSpeed;
                    stored = true;
                }
                player.locomotion.airSpeed *= mult;
                player.locomotion.forwardSpeed *= mult;
                player.locomotion.horizontalSpeed *= mult;
                player.locomotion.verticalSpeed *= mult;
                player.locomotion.runSpeedAdd *= mult;
                player.locomotion.strafeSpeed *= mult;
                player.locomotion.backwardSpeed *= mult;
                instance = Catalog.GetData<EffectData>("MonadoSpeedAura").Spawn(player.transform);
                instance.SetRenderer(player.creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || player.creature.isKilled)
                {
                    Destroy(this);
                }
            }
            public void OnDestroy()
            {
                player.locomotion.airSpeed /= mult;
                player.locomotion.forwardSpeed /= mult;
                player.locomotion.horizontalSpeed /= mult;
                player.locomotion.verticalSpeed /= mult;
                player.locomotion.runSpeedAdd /= mult;
                player.locomotion.strafeSpeed /= mult;
                player.locomotion.backwardSpeed /= mult;
                instance.Stop();
            }
        }
        public class PurgeAura : MonoBehaviour
        {
            Creature creature;
            float timer;
            EffectInstance instance;
            float auraTime;
            public void Setup(float time)
            {
                auraTime = time;
            }
            public void Start()
            {
                creature = GetComponent<Creature>();
                timer = Time.time;
                creature.TryElectrocute(1, 3, true, false, Catalog.GetData<EffectData>("ImbueLightningRagdoll", true));
                instance = Catalog.GetData<EffectData>("MonadoPurgeAura").Spawn(creature.transform);
                instance.SetRenderer(creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || creature.isKilled)
                {
                    instance.Stop();
                    Destroy(this);
                }
                else
                {
                    creature.mana.currentMana = 0f;
                    Destroy(creature.gameObject.GetComponent<EnchantAura>());
                    Destroy(creature.gameObject.GetComponent<ShieldAura>());
                    Destroy(creature.gameObject.GetComponent<SpeedAura>());
                    Destroy(creature.gameObject.GetComponent<ArmorAura>());
                }
            }
        }
        public class CycloneAura : MonoBehaviour
        {
            Creature creature;
            float timer;
            float toppleReset;
            EffectInstance instance;
            float auraTime;
            public void Setup(float time)
            {
                auraTime = time;
            }
            public void Start()
            {
                creature = GetComponent<Creature>();
                timer = Time.time;
                toppleReset = Time.time;
                creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                instance = Catalog.GetData<EffectData>("MonadoCycloneAura").Spawn(creature.transform);
                instance.SetRenderer(creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || creature.isKilled)
                {
                    Destroy(this);
                }
                else if (Time.time - toppleReset >= 1)
                {
                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                    toppleReset = Time.time;
                }
            }
            public void OnDestroy()
            {
                instance.Stop();
            }
        }
        public class ArmorAura : MonoBehaviour
        {
            Creature creature;
            float timer;
            EffectInstance instance;
            float auraTime;
            float mult;
            public void Setup(float time, float multiplier)
            {
                auraTime = time;
                mult = multiplier;
                if (mult > 1) mult = 1;
                else if (mult < 0) mult = 0;
            }
            public void Start()
            {
                creature = GetComponent<Creature>();
                creature.OnDamageEvent += Creature_OnDamageEvent;
                timer = Time.time;
                instance = Catalog.GetData<EffectData>("MonadoArmorAura").Spawn(creature.transform);
                instance.SetRenderer(creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || creature.isKilled)
                {
                    Destroy(this);
                }
            }
            private void Creature_OnDamageEvent(CollisionInstance collisionInstance)
            {
                if (!collisionInstance.ignoreDamage)
                    creature.currentHealth += collisionInstance.damageStruct.damage * mult;
            }
            public void OnDestroy()
            {
                creature.OnDamageEvent -= Creature_OnDamageEvent;
                instance.Stop();
            }
        }
        public class EaterAura : MonoBehaviour
        {
            Creature creature;
            float timer;
            float cooldown;
            EffectInstance instance;
            float auraTime;
            float dot; 
            public void Setup(float time, float damage)
            {
                auraTime = time;
                dot = damage;
            }
            public void Start()
            {
                creature = GetComponent<Creature>();
                timer = Time.time;
                cooldown = Time.time;
                instance = Catalog.GetData<EffectData>("MonadoEaterAura").Spawn(creature.transform);
                instance.SetRenderer(creature.GetRendererForVFX(), false);
                instance.SetIntensity(1f);
                instance.Play();
                Destroy(creature.gameObject.GetComponent<EnchantAura>());
                Destroy(creature.gameObject.GetComponent<ShieldAura>());
                Destroy(creature.gameObject.GetComponent<SpeedAura>());
                Destroy(creature.gameObject.GetComponent<ArmorAura>());
                creature.ragdoll.AddPhysicToggleModifier(this);
            }
            public void FixedUpdate()
            {
                if (Time.time - timer >= auraTime || creature.isKilled)
                {
                    instance.Stop();
                    creature.ragdoll.RemovePhysicToggleModifier(this);
                    Destroy(this);
                }
                else if(Time.time - cooldown >= 1.5f)
                {
                    CollisionInstance collision = new CollisionInstance(new DamageStruct(DamageType.Slash, dot));
                    collision.damageStruct.hitRagdollPart = creature.ragdoll.rootPart;
                    cooldown = Time.time;
                    creature.Damage(collision);
                }
            }
        }
    }
}
