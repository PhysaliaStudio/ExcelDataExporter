// ###############################################
// #### AUTO GENERATED CODE, DO NOT MODIFY!! #####
// ###############################################

using System;
using Physalia.ExcelDataExporter;
using UnityEngine;

namespace TestGame
{
    [Serializable]
    public class ValidExampleVerticalData : IHasId
    {
        [SerializeField]
        private int _id;
        [SerializeField]
        private string _name;
        [SerializeField]
        private int[] _attack;
        [SerializeField]
        private bool _type;
        [SerializeField]
        private Vector2Int[] _startPos;

        public int Id => _id;
        public string Name => _name;
        public int[] Attack => _attack;
        public bool Type => _type;
        public Vector2Int[] StartPos => _startPos;
    }
}
