using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class VivoxController : MonoBehaviour
{
    private async void Awake()
    {
        //유니티 서비스 초기화
        await UnityServices.InitializeAsync();

        //AuthenticationService를 사용하여 익명 인증
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //Vivox 초기화
        await VivoxService.Instance.InitializeAsync();

        Debug.Log("초기화 완료");

        await LoginAsync();

        Debug.Log("로그인 완료");

    }

    private async Task LoginAsync()
    {

        //로그인 옵션 생성
        LoginOptions options = new LoginOptions();

        //디스플레이 이름 설정
        options.DisplayName = Guid.NewGuid().ToString();

        //로그인
        await VivoxService.Instance.LoginAsync(options);
    }
}
