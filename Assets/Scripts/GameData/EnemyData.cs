// ###############################################
// #### AUTO GENERATED CODE, DO NOT MODIFY!! #####
// ###############################################

using System;
using Physalia.ExcelDataExporter;
using UnityEngine;

namespace TestGame.BattleSystem
{
    [Serializable]
    public class EnemyData : IHasId
    {
        [SerializeField]
        private int _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private EnemyType _type;
        [SerializeField]
        private int[] _attack;
        [SerializeField]
        private Vector2Int[] _startPos;
        [SerializeField]
        private Reward _reward;

        public int Id => _id;
        public string Name => _name;

        /// <summary>
        /// Enemy Type
        /// </summary>
        public EnemyType Type => _type;

        /// <summary>
        /// ATK for each level
        /// </summary>
        public int[] Attack => _attack;

        /// <summary>
        /// Spawn Points
        /// </summary>
        public Vector2Int[] StartPos => _startPos;

        /// <summary>
        /// Fixed Item Drop
        /// </summary>
        public Reward Reward => _reward;
    }
}
