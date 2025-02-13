﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Runtime.InteropServices;

namespace PlayUR.Core
{
    /// <summary>
    /// Singleton in charge of displaying the login screen. 
    /// GUI not actually used on the webpage, but allows standalone exes to integrate with the system.
    /// Login system on the webpage actually auto-logs in using some functions in this class.
    /// </summary>
    public class PlayURLoginCanvas : UnitySingleton<PlayURLoginCanvas>
    {

        #region Constants
#if UNITY_EDITOR || !UNITY_WEB_GL
        bool ENABLE_PERSISTENCE = true;
#else
        bool ENABLE_PERSISTENCE = false;
#endif
        #endregion

        #region GUI Links
        public InputField username, password;
        public Text feedback;
        public Button submit, register;
        public GameObject loginScreen, registerScreen;
        public GameObject fullscreenError;
        public Text errorText, errorTitle;
        public Button errorOK;

        public InputField registerUsername, registerPassword, registerConfirmPassword, registerEmail, registerFirstName, registerLastName;
        public Button registerSubmit, registerCancel;
        public Text registerFeedback;
        #endregion

        #region State and Set Up
        /// <summary>
        /// Returns if we have successfully logged in or not
        /// </summary>
        public static bool LoggedIn { get; private set; }
        
        //this var either contains the text put in the password field, or is populated by the auto-login process.
        string pwd;

        //should we attempt to auto login? turn this flag off once we fail on an auto
        static bool autoLogin = true;
        //use this to keep a message between loads of the login scene
        static string persistFeedbackMessage = "";

        void Awake()
        {
            GetComponent<CanvasGroup>().alpha = 0;
        }
        private void Start()
        {
            if (!string.IsNullOrEmpty(persistFeedbackMessage))
                feedback.text = persistFeedbackMessage;

            submit.onClick.AddListener(() => { pwd = password.text; Login(); });
            register.onClick.AddListener(() => { OpenRegister(); });
            registerCancel.onClick.AddListener(() => { CloseRegister(); });
            registerSubmit.onClick.AddListener(() => { Register(); });

            if (ENABLE_PERSISTENCE && autoLogin)
            {
                if (UnityEngine.PlayerPrefs.HasKey(PlayURPlugin.PERSIST_KEY_PREFIX + "username"))
                    username.text = UnityEngine.PlayerPrefs.GetString(PlayURPlugin.PERSIST_KEY_PREFIX + "username");

                if (UnityEngine.PlayerPrefs.HasKey(PlayURPlugin.PERSIST_KEY_PREFIX + "password"))
                {
                    pwd = UnityEngine.PlayerPrefs.GetString(PlayURPlugin.PERSIST_KEY_PREFIX + "password");
                    PlayURPlugin.Log("Auto-login...");
                    Login();
                }
            }

            #if UNITY_WEBGL && !UNITY_EDITOR
            try
            {
                RequestWebGLLogin();
            } catch (System.EntryPointNotFoundException) { }
            #else
            GetComponent<CanvasGroup>().alpha = 1;
            #endif
        }

        /// <summary>
        /// Triggers a login request with whatever username and password has been entered.
        /// </summary>
        public void Login()
        {
            feedback.text = "Logging in... ";
            if (ENABLE_PERSISTENCE)
            {
                UnityEngine.PlayerPrefs.SetString(PlayURPlugin.PERSIST_KEY_PREFIX + "username", username.text);
            }

            PlayURPlugin.instance.Login(username.text, pwd, (succ, result) =>
            {
                password.text = string.Empty;
                PlayURPlugin.Log("Login Success: "+ succ);
                if (succ)
                {
                    //TODO: security ??
                    if (ENABLE_PERSISTENCE)
                    {
                        UnityEngine.PlayerPrefs.SetString(PlayURPlugin.PERSIST_KEY_PREFIX + "password", pwd);
                    }
                    LoggedIn = true;
                    if (PlayURPluginHelper.startedFromScene > 0)
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(PlayURPluginHelper.startedFromScene);
                    }
                    else
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
                    }
                }
                else
                {
                    GetComponent<CanvasGroup>().alpha = 1;
                    feedback.text = "Incorrect Username or Password";//todo pull from server?
                }
            });
        }
        public void CancelLogin(string message = "Could not login. Contact the researcher.")
        {
            LoggedIn = false;
            autoLogin = false;
            persistFeedbackMessage = message;
        }

        void OpenRegister()
        {
            loginScreen.SetActive(false);
            registerScreen.SetActive(true);
            registerUsername.text = username.text;
            registerPassword.text = password.text;
            registerConfirmPassword.text = password.text;

            registerUsername.Select();
            registerUsername.ActivateInputField();
        }
        void CloseRegister()
        {
            loginScreen.SetActive(true);
            registerScreen.SetActive(false);

            username.Select();
            username.ActivateInputField();
        }


        /// <summary>
        /// Triggers a register request with whatever details has been entered.
        /// </summary>
        public void Register()
        {
            registerFeedback.text = "Registering... ";

            if (registerUsername.text.Length == 0)
            {
                registerFeedback.text = "No username entered";
            }
            else if (registerEmail.text.Length == 0)
            {
                registerFeedback.text = "No email entered";
            }
            else if (registerPassword.text != registerConfirmPassword.text)
            {
                registerFeedback.text = "Passwords entered do not match!";
            }
            else if (registerPassword.text.Length < 3)
            {
                registerFeedback.text = "Password too short!";
            }
            else
            {

                PlayURPlugin.instance.Register(registerUsername.text, registerPassword.text, registerEmail.text, registerFirstName.text, registerLastName.text, (succ, result) =>
                {
                    PlayURPlugin.Log("Register Success: " + succ+"'"+ result["message"]+"'");
                    if (succ)
                    {
                        username.text = registerUsername.text;
                        password.text = registerPassword.text;
                        pwd = password.text;
                        registerScreen.SetActive(false);
                        Login();
                    }
                    else
                    {
                        GetComponent<CanvasGroup>().alpha = 1;
                        registerFeedback.text = "Registration Failed: "+ result["message"];
                    }
                });
            }
        }
        #endregion

        #region WebGLLinkage
        /// <summary>
        /// This function has a matching JavaScript function on the website which gets called when we call this function from C#
        /// Slightly convoluated set up uses this, as the webpage otherwise doesn't know when the <see cref="PlayURLoginCanvas"/>
        /// is ready to call <see cref="WebGLLogin(string)"/>. 
        /// </summary>
        [DllImport("__Internal")]
        private static extern void RequestWebGLLogin();

        /// <summary>
        /// The website will call this function (inside <see cref="RequestWebGLLogin"/> in JavaScript).
        /// </summary>
        /// <param name="jsonInput">The username and password of the user.</param>
        public void WebGLLogin(string jsonInput)
        {
            var jsonData = JSON.Parse(jsonInput);
            username.text = jsonData["username"];
            pwd = jsonData["password"];

            PlayURPlugin.browserInfo = jsonData["browserInfo"];
            
            var e = jsonData["experiment"];
            int i = -1;
            int.TryParse(e, out i);
            if (i != -1)
            {
                PlayURPlugin.instance.didRequestExperiment = true;
                PlayURPlugin.instance.requestedExperiment = (Experiment)i;
            }
            e = jsonData["experimentGroup"];
            i = -1;
            int.TryParse(e, out i);
            if (i != -1)
            {
                PlayURPlugin.instance.didRequestExperimentGroup = true;
                PlayURPlugin.instance.requestedExperimentGroup = (ExperimentGroup)i;
            }

            //attempt to login with the info given by the site!
            Login();
        }

        #endregion

        #region Error Display
        /// <summary>
        /// Displays a full-screen error, and prevents user from leaving this scene
        /// </summary>
        /// <param name="message">Body text of error popup</param>
        /// <param name="title">Title text of error popup</param>
        /// <param name="showOK">Show an OK button which user can press to try and log in again?</param>
        public void ShowError(string message, string title = "Error", bool showOK = false)
        {
            fullscreenError.SetActive(true);
            errorOK.onClick.AddListener(() => {
                fullscreenError.SetActive(false);
            });
            errorOK.gameObject.SetActive(showOK);
            errorTitle.text = title;
            errorText.text = message;
        }
        #endregion
    }



}