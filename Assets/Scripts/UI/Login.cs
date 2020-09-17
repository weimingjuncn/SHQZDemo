using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
    private InputField _inputAccount;
    private InputField _inputPassword;
    private Button _LoginButton;
    // Start is called before the first frame update
    void Start()
    {
        _inputAccount = transform.Find("InputAccount").GetComponent<InputField>();
        _inputPassword = transform.Find("InputPassword").GetComponent<InputField>();
        _LoginButton = transform.Find("LoginButton").GetComponent<Button>();

        _LoginButton.onClick.AddListener(OnclickEnter);
    }

    private void OnclickEnter()
    {
        SceneManager.LoadScene("HeroSelection");
        //Debug.Log("登录成功！");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
