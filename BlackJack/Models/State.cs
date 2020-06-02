using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlackJack.Models
{
    public enum EndResult
    {
        DealerBlackJack,
        PlayerBlackJack,
        PlayerBust,
        DealerBust,
        Push,
        PlayerWin,
        DealerWin
    }
}
