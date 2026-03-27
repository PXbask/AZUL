using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AZUL
{
    public class ColoredPlaceTokenArea : PlaceTokenArea
    {
        [SerializeField]
        private PieceColorType colorType;

        public PieceColorType ColorType => colorType;
    }
}
