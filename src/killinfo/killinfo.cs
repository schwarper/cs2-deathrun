using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.InteropServices;
using static Deathrun.Deathrun;

namespace Deathrun;

public static class KillInfo
{
    public static void Create(CCSPlayerController victim, CCSPlayerController attacker)
    {
        int size = Schema.GetClassSize("CTakeDamageInfo");
        nint ptr = Marshal.AllocHGlobal(size);

        for (int i = 0; i < size; i++)
        {
            Marshal.WriteByte(ptr, i, 0);
        }

        CTakeDamageInfo damageInfo = new(ptr);
        CAttackerInfo attackerInfo = new(PlayerTerrorist!);
        Marshal.StructureToPtr(attackerInfo, new IntPtr(ptr.ToInt64() + 0x88), false);

        Schema.SetSchemaValue(damageInfo.Handle, "CTakeDamageInfo", "m_hInflictor", attacker.Pawn.Raw);
        Schema.SetSchemaValue(damageInfo.Handle, "CTakeDamageInfo", "m_hAttacker", attacker.Pawn.Raw);
        damageInfo.Damage = 9999;

        VirtualFunctions.CBaseEntity_TakeDamageOldFunc.Invoke(victim.Pawn.Value!, damageInfo);
        Marshal.FreeHGlobal(ptr);
    }
}

[StructLayout(LayoutKind.Explicit)]
public struct CAttackerInfo
{
    public CAttackerInfo(CEntityInstance attacker)
    {
        NeedInit = false;
        IsWorld = true;
        Attacker = attacker.EntityHandle.Raw;
        if (attacker.DesignerName != "cs_player_controller") return;

        CCSPlayerController controller = attacker.As<CCSPlayerController>();
        IsWorld = false;
        IsPawn = true;
        AttackerUserId = (ushort)(controller.UserId ?? 0xFFFF);
        TeamNum = controller.TeamNum;
        TeamChecked = controller.TeamNum;
    }

    [FieldOffset(0x0)] public bool NeedInit = true;
    [FieldOffset(0x1)] public bool IsPawn = false;
    [FieldOffset(0x2)] public bool IsWorld = false;

    [FieldOffset(0x4)]
    public UInt32 Attacker;

    [FieldOffset(0x8)]
    public ushort AttackerUserId;

    [FieldOffset(0x0C)] public int TeamChecked = -1;
    [FieldOffset(0x10)] public int TeamNum = -1;
}