using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Penumbra.Api.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static OopsAllLalafellsSRE.Utils.Constant;

namespace OopsAllLalafellsSRE.Utils
{
    internal class Drawer : IDisposable
    {
        public Drawer()
        {
            Service.configWindow.OnConfigChanged += RefreshAllPlayers;
            Service.configWindow.OnConfigChangedSingleChar += RefreshOnePlayer;
            if (Service.configuration.enabled)
            {
                Plugin.OutputChatLine("OopsAllNaked starting...");
                RefreshAllPlayers();
            }
        }

        private static void RefreshAllPlayers()
        {
            Plugin.OutputChatLine("Refreshing all players");

            foreach (var obj in Service.objectTable)
            {
                if (!obj.IsValid()) continue;
                if (obj is not ICharacter) continue;
                if (Service.configuration.IsWhitelisted(obj.Name.TextValue)) continue;

                bool isSelf = obj.ObjectIndex == 0 || obj.ObjectIndex == 201;
                if (Service.configuration.dontLalaSelf && Service.configuration.dontStripSelf && isSelf) continue;

                Service.penumbraApi.RedrawOne(obj.ObjectIndex, RedrawType.Redraw);
            }
        }

        private static void RefreshOnePlayer(string charName)
        {
            if (!Service.configuration.enabled)
                return;

            int objectIndex = -1;

            foreach (var obj in Service.objectTable)
            {
                if (!obj.IsValid()) continue;
                if (obj is not ICharacter) continue;
                if (obj.Name.TextValue != charName) continue;
                objectIndex = obj.ObjectIndex;
                break;
            }

            if (objectIndex == -1)
                return;

            Service.penumbraApi.RedrawOne(objectIndex, RedrawType.Redraw);
        }

        public static unsafe void OnCreatingCharacterBase(nint gameObjectAddress, Guid _1, nint _2, nint customizePtr, nint equipPtr)
        {
            if (!Service.configuration.enabled) return;

            // return if not player character
            var gameObj = (GameObject*)gameObjectAddress;
            if (gameObj->ObjectKind != ObjectKind.Pc) return;

            var customData = Marshal.PtrToStructure<CharaCustomizeData>(customizePtr);
            var equipData = (ulong*)equipPtr;

            var charName = gameObj->NameString;

            bool isSelf = false;

            if (gameObj->ObjectIndex == 0 || gameObj->ObjectIndex == 201)
                isSelf = true;

            bool dontLala = Service.configuration.dontLalaSelf && isSelf;
            bool dontStrip = Service.configuration.dontStripSelf && isSelf;

            if (Service.configuration.IsWhitelisted(charName))
                return;

            if (customData.Race == Service.configuration.SelectedRace || customData.Race == Race.UNKNOWN)
                dontLala = true;

            if (dontLala && dontStrip)
                return;

            if (!dontLala && Service.configuration.SelectedRace != Race.UNKNOWN)
                ChangeRace(customData, customizePtr, Service.configuration.SelectedRace);

            if (!dontStrip)
                StripClothes(equipData, equipPtr);
        }

        private static unsafe void ChangeRace(CharaCustomizeData customData, nint customizePtr, Race selectedRace)
        {
            customData.Race = selectedRace;
            customData.Tribe = (byte)(((byte)selectedRace * 2) - (customData.Tribe % 2));
            customData.FaceType %= 4;
            customData.ModelType %= 2;
            customData.HairStyle = (byte)((customData.HairStyle % RaceMappings.RaceHairs[selectedRace]) + 1);
            Marshal.StructureToPtr(customData, customizePtr, true);
        }

        private static unsafe void StripClothes(ulong* equipData, nint equipPtr)
        {
            if (Service.configuration.stripHats) equipData[0] = 0;
            if (Service.configuration.stripBodies) equipData[1] = 0;
            if (Service.configuration.stripGloves) equipData[2] = 0;
            if (Service.configuration.stripLegs) equipData[3] = 0;
            if (Service.configuration.stripBoots) equipData[4] = 0;
            if (Service.configuration.stripAccessories)
            {
                for (int i = 5; i <= 9; ++i)
                    equipData[i] = 0;
            }
        }

        public void Dispose()
        {
            Service.configWindow.OnConfigChanged -= RefreshAllPlayers;
            Service.configWindow.OnConfigChangedSingleChar -= RefreshOnePlayer;
        }
    }
}