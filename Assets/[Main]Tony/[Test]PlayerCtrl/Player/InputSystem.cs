using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface IInput {
    public Vector2 MoveJoy();
    public Vector2 AimJoy();
}

public class PCInput : IInput
{
    [Inject] 
    private IBattleCtrl BattleCtrl;
    
    public Vector2 MoveJoy() {
        Vector2 data = new Vector2();
        if (Input.GetKey(KeyCode.A)) data += new Vector2(-1,  0);
        if (Input.GetKey(KeyCode.D)) data += new Vector2( 1,  0);
        if (Input.GetKey(KeyCode.W)) data += new Vector2( 0,  1);
        if (Input.GetKey(KeyCode.S)) data += new Vector2( 0, -1);
        return data.normalized;
    }

    public Vector2 AimJoy()
    {
        Vector2 data = Vector2.zero;
        if (Input.GetMouseButton(0)) {
            var localPlayer = BattleCtrl.GetLocalPlayer();
            if(!localPlayer) return Vector2.zero;
            Vector3 playerPos = localPlayer.transform.position;
            playerPos = Camera.main.WorldToScreenPoint(playerPos);
            Vector3 mousePos = Input.mousePosition;
            data = mousePos - playerPos;
        }
        return data.normalized;
    }
}

public class AndroidInput : IInput {
    public Vector2 MoveJoy() {
        Vector2 data = new Vector2();
        //todo
        return data;
    }

    public Vector2 AimJoy()
    {
        Vector2 data = new Vector2();
        //todo
        return data;
    }
}

public class IPhoneInput : IInput {
    public Vector2 MoveJoy() {
        Vector2 data = new Vector2();
        //todo
        return data;
    }
    public Vector2 AimJoy()
    {
        Vector2 data = new Vector2();
        //todo
        return data;
    }
}

public class InputSystem : MonoInstaller {
    public override void InstallBindings() {
        switch (Application.platform) {
            case RuntimePlatform.IPhonePlayer:
                Container.Bind<IInput>().To<IPhoneInput>().AsSingle();
                break;
            case RuntimePlatform.Android:
                Container.Bind<IInput>().To<AndroidInput>().AsSingle();
                break;
            default:
                Container.Bind<IInput>().To<PCInput>().AsSingle();
                break;
        }
    }
}

