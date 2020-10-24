﻿using System.Timers;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;

namespace Listener.Player.Spotify
{
    internal class SpotifyAuthentication
    {
        public delegate void SpotifyEventHandler(SpotifyAuthentication sender, SpotifyWebAPI client);
        public event SpotifyEventHandler OnClientReady;
        private readonly AuthorizationCodeAuth AuthenFactory;
        private readonly Timer _refreshTokenWorker;
        private string RefreshToken;
        private SpotifyWebAPI Client
        { get; set; }
        public static SpotifyAuthentication Context { get; }
        static SpotifyAuthentication()
        {
            Context = new SpotifyAuthentication();
        }
        private SpotifyAuthentication()
        {
            bool initialized = false;
            var client_id = "7b2f38e47869431caeda389929a1908e";
            var secret_id = "c3a86330ef844c16be6cb46d5e285a45";
            AuthenFactory = new AuthorizationCodeAuth(
                                client_id,
                                secret_id,
                                "http://localhost:8800",
                                "http://localhost:8800",
                                Scope.UserReadCurrentlyPlaying |
                                Scope.UserModifyPlaybackState |
                                Scope.AppRemoteControl |
                                Scope.UserReadPlaybackState
                                );
            AuthenFactory.AuthReceived += async (s, p) =>
            {
                var ath = (AuthorizationCodeAuth)s;
                ath.Stop();

                var token = await ath.ExchangeCode(p.Code);
                RefreshToken = token.RefreshToken;
                if (Client == null)
                {
                    Client = new SpotifyWebAPI()
                    {
                        AccessToken = token.AccessToken,
                        TokenType = "Bearer"
                    };
                }
                else
                {
                    Client.AccessToken = token.AccessToken;
                }
                if (!initialized)
                    OnClientReady.Invoke(this, Client);
                initialized = true;

            };
            AuthenFactory.Start();
            AuthenFactory.OpenBrowser();
            _refreshTokenWorker = new Timer();
            _refreshTokenWorker.Interval = 30 * (1000 * 60);
            _refreshTokenWorker.Elapsed += async (s, e) =>
            {
                var token = await AuthenFactory.RefreshToken(RefreshToken);
                Client.AccessToken = token.AccessToken;
            };
            _refreshTokenWorker.Start();
        }
    }
}
