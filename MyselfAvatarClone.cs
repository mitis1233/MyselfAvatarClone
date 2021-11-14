using ExitGames.Client.Photon;
using MelonLoader;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.DataModel;
using System.Linq;
using System;
using System.Collections;

[assembly: MelonGame("VRChat")]
[assembly: MelonInfo(typeof(MyselfAvatarClone.MyselfAvatarClone), "MyselfAvatarClone", "1.0.1")]

namespace MyselfAvatarClone
{
    public class MyselfAvatarClone : MelonMod
    {
        private static bool State = false;
        private static Il2CppSystem.Object avatarDictCache { get; set; }
        GameObject quickMenu;

        private static void Log(string message)
        {
            VRCUiManager.prop_VRCUiManager_0.field_Private_List_1_String_0.Add(message);
        }

        private static void Detour(ref EventData __0)
        {
            if (State
                && __0.Code == 253
                && avatarDictCache != null
                && __0.Sender == VRC.Player.prop_Player_0.field_Private_VRCPlayerApi_0.playerId
            ) __0.Parameters[251].Cast<Il2CppSystem.Collections.Hashtable>()["avatarDict"] = avatarDictCache;
        }

        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(QMInitializer());
            HarmonyInstance.Patch(
                typeof(VRCNetworkingClient).GetMethod(nameof(VRCNetworkingClient.OnEvent)),
                typeof(MyselfAvatarClone).GetMethod(nameof(Detour), BindingFlags.NonPublic | BindingFlags.Static).ToNewHarmonyMethod()
            );
        }

        private IEnumerator QMInitializer()
        {
            while ((quickMenu = GameObject.Find("UserInterface/Canvas_QuickMenu(Clone)")) == null)
                yield return null;
            SelMenu();
        }
        
        private void SelMenu()
        {
            Transform ButtonTP = quickMenu.transform.Find("Container/Window/QMParent/Menu_SelectedUser_Local/ScrollRect/Viewport/VerticalLayoutGroup/Buttons_UserActions");
            Utils.CreateDefaultButton("複製Avatar", new Vector3(0, -25, 0), "顯示'成功'後 隨便更換一次Avatar即可複製 僅有自己可見", Color.white, ButtonTP,
                new Action(() => {
                    string target = string.Empty;
                    if (UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1 == null)
                    {
                        return;
                    }
                    else target = UserSelectionManager.field_Private_Static_UserSelectionManager_0.field_Private_APIUser_1.id;

                    avatarDictCache = VRC.PlayerManager.prop_PlayerManager_0
                        .field_Private_List_1_Player_0
                        .ToArray()
                        .Where(a => a.field_Private_APIUser_0.id == target)
                        .FirstOrDefault()
                        .prop_Player_1.field_Private_Hashtable_0["avatarDict"];
                    Log("成功");
                }));
            Utils.CreateDefaultButton("開關自嗨Avatar顯示", new Vector3(0, -25, 0), "開關複製Avatar功能", Color.white, ButtonTP,
                new Action(() => {
                    State ^= true;
                    Log("SoftClone " + (State ? "自嗨功能開啟" : "自嗨功能關閉"));
                }));
        }
    }
}
