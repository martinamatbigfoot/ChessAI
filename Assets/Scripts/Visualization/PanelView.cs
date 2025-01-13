using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RookWorks.Visualization
{
    public class PanelView : MonoBehaviour
    {
        [SerializeField]
        private GameObject _mainContainer;
        [SerializeField]
        private GameObject _gameContainer;
        [SerializeField]
        private Button _startGameButton;
        [SerializeField]
        private Button _loadGamesButton;
        [SerializeField]
        private Button _endGameButton;
        [SerializeField]
        private GameObject _gamesContainer;
        [SerializeField]
        private Transform _gamesParent;
        [SerializeField]
        private GameObject _gameViewPrefab;

        private Action<Guid> _startGameCallback;
        
        public void Initialize(Action<Guid> startGame, Action loadGames)
        {
            EndGame();
            _startGameCallback = startGame;
            _startGameButton.onClick.AddListener(StartGame);
            _loadGamesButton.onClick.AddListener(new UnityAction(loadGames));
            _endGameButton.onClick.AddListener(EndGame);
        }

        public void ShowLoadedGames(List<Dictionary<string, string>> games)
        {
            
            foreach (var game in games)
            {
                GameDataView gameView = Instantiate(_gameViewPrefab, _gamesParent).GetComponent<GameDataView>();
                gameView.Initialize(game, LoadGame);
            }
            _gamesContainer.SetActive(true);
        }

        private void EndGame()
        {
            _mainContainer.SetActive(true);
            _gameContainer.SetActive(false);
        }

        void StartGame()
        {
            LoadGame(new Guid());
        }

        void LoadGame(Guid guid)
        {
            _startGameCallback?.Invoke(guid);
            _mainContainer.SetActive(false);
            _gameContainer.SetActive(true);
        }
    }
}
