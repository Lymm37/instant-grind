using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace InstantGrind
{
    [BepInPlugin("Lymm37.PotionCraft.InstantGrind", "InstantGrind", "1.0.0")]
    [BepInProcess("Potion Craft.exe")]
    public class InstantGrind : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("Lymm37.PotionCraft.InstantGrind");

        void Awake()
        {
            Debug.Log($"[Instant Grind] Patching...");
            harmony.PatchAll();
            Debug.Log($"[Instant Grind] Patching complete.");
        }

        
        

        // Grinding status can be set to the end, which will then update the map
        [HarmonyPatch(typeof(ObjectBased.Stack.Stack))]
        [HarmonyPatch("UpdateOverallGrindStatus")]
        class Stack_Grind_Patch
        {
            // Skip the grinding status update if fully ground, to prevent it from going backward
            static bool Prefix(ref float ___overallGrindStatus)
            {
                return !(___overallGrindStatus == 1f);
            }
            // Postfix because the grind status gets set at the end
            static void Postfix(ref float ___leavesGrindStatus, ref float ___overallGrindStatus)
            {
                // Only fully grind when right click is held
                if (Input.GetMouseButton(1))
                {
                    ___leavesGrindStatus = 1f;
                    ___overallGrindStatus = 1f;
                    // No need to clutter the logs
                    //Debug.Log($"[Instant Grind] Grinded ingredient");
                }
            }
            /*
            static void Prefix(ObjectBased.Stack.Stack __instance, ref float ___leavesGrindStatus)
            {
                //float GrindTicksToFullGrindRef = (float)Traverse.Create(typeof(ObjectBased.Stack.SubstanceGrinding)).Field("GrindTicksToFullGrind").GetValue();
                Debug.Log($"[Instant Grind] Setting leaves to fully ground...");
                //this.Ingredient.substanceGrindingSettings.percentOfSubstanceGrinding
                float perc = __instance.Ingredient.substanceGrindingSettings.percentOfSubstanceGrinding;
                ___leavesGrindStatus = 1f;
                float overall = ___leavesGrindStatus * (1 - perc) + __instance.substanceGrinding.CurrentGrindStatus * perc;
                Debug.Log($"[Instant Grind] {overall}");
            }
            */
        }

        // Skips grinding cooldown (probably not necessary anymore)
        /*
        [HarmonyPatch(typeof(IngredientFromStack))]
        [HarmonyPatch("CooledDown")]
        class Cooldown_Patch
        {
            static bool Prefix(ref bool __result)
            {
                Debug.Log($"[Instant Grind] Bypassed cooldown");
                __result = true;
                return false;
            }
        }
        */

        // This sets the powder to be fully ground
        [HarmonyPatch(typeof(ObjectBased.Stack.SubstanceGrinding))]
        [HarmonyPatch("TryToGrind")]
        class Substance_Grind_Patch
        {
            static void Prefix(ObjectBased.Stack.SubstanceGrinding __instance, ref float ___grindTicksPerformed, ref float ____currentGrindStatus)
            {
                //float GrindTicksToFullGrindRef = (float)Traverse.Create(typeof(ObjectBased.Stack.SubstanceGrinding)).Field("GrindTicksToFullGrind").GetValue();
                //Debug.Log($"[Instant Grind] Setting to fully ground...");
                if (Input.GetMouseButton(1))
                {
                    ___grindTicksPerformed = GetPrivateProperty<int>(__instance, "GrindTicksToFullGrind");
                    ____currentGrindStatus = 1f;
                }
                //Debug.Log($"[Instant Grind] Done");
            }
        }
        

        // This only makes it appear more ground, while moving a bit on the map. It also makes throwing it at the wall just delete the ingredient...
        /*
        [HarmonyPatch(typeof(IngredientFromStack))]
        [HarmonyPatch("TryToGrind")]
        class Ingredient_Grind_Patch
        {
            static void Prefix(IngredientFromStack __instance)
            {

                Debug.Log($"[Instant Grind] Modifying grinding...");
                if (__instance is not null)
                {
                    Debug.Log($"[Instant Grind] Instance is not null");
                    //__instance.NextGrindState();
                    //Traverse nextGrindStateMethod = Traverse.Create(typeof(IngredientFromStack)).Method("NextGrindState");
                    //nextGrindStateMethod.Invoke
                    //__instance.Invoke("NextGrindState", 0);
                    // Arbitrary choice to increase grint state 10 times...
                    for (int i = 0; i < 10; i++) { 
                        InvokePrivateMethod(__instance, "NextGrindState");
                    }
                    Debug.Log($"[Instant Grind] Increased grind state.");
                }
                // These variables are unused, despite having documentation that makes it seem like they were originally used to fully grind things instantly for debugging
                //Traverse.Create(typeof(PestleGrindingZone)).Field("strikeMaxDamage").SetValue(1000f);
                //Traverse.Create(typeof(PestleGrindingZone)).Field("rubMaxDamage").SetValue(1000f);
            }
        }
        */

        // Helper functions to access private fields, properties, and methods

        public static object InvokePrivateMethod(object instance, string methodName, params object[] parameters)
        {
            var method = instance.GetType().GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method is not null)
            {
                return method.Invoke(instance, parameters);
            }
            return null;
        }

        public static void SetPrivateField<T>(object instance, string fieldName, T value)
        {
            var prop = instance.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            prop.SetValue(instance, value);
        }

        public static T GetPrivateProperty<T>(object instance, string propertyName)
        {
            var prop = instance.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)prop.GetValue(instance);
        }
    }
}
