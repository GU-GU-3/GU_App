using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectAdvice
{
    /// <summary>
    /// SlackAPIからのレスポンスを表すクラス
    /// </summary>
    public class SlackResponse
    {
        #region プロパティ

        /// <summary>
        /// メッセージリスト
        /// </summary>
        public List<Message> messages { get; set; }

	    #endregion
    }
}
