//**********************************************************************************
//* フレームワーク・テスト画面（Ｐ層）
//**********************************************************************************

// テスト画面なので、必要に応じて流用 or 削除して下さい。

//**********************************************************************************
//* クラス名        ：testBlankScreen
//* クラス日本語名  ：ブランクのMaster Page
//*
//* 作成日時        ：－
//* 作成者          ：生技
//* 更新履歴        ：
//*
//*  日時        更新者            内容
//*  ----------  ----------------  -------------------------------------------------
//*  20xx/xx/xx  ＸＸ ＸＸ         ＸＸＸＸ
//**********************************************************************************

using System;

using Touryo.Infrastructure.Business.Util;
using Touryo.Infrastructure.Framework.Presentation;
using Touryo.Infrastructure.Framework.Util;

namespace WebForms_Sample.Aspx.Common.Master
{
    /// <summary>ブランクのMaster Page</summary>
    public partial class testBlankScreen : BaseMasterController
    {
        // 共通仕様により、すべての画面がフッタ部にメイン ボタンを5つ持つ。
        // 既定（未使用＝キャプション "－"・非活性）は .master の宣言で与え、
        // 使う画面が各々の初期処理（UOC_FormInit）で上書きする。
        // ★ ここで Page_Load から既定値を設定してはいけない。マスタ ページの Load は
        //   コンテンツ ページの UOC_FormInit より後に走るため、画面が設定した
        //   キャプションを毎回上書きしてしまう（ボタンが常に "－"・非活性に見える）。
        /// <summary>UserName</summary>
        public string UserName
        {
            get
            {
                var user = (MyUserInfo)UserInfoHandle.GetUserInformation();

                if (user == null)
                {
                    return "anonymous";
                }
                else
                {
                    return user.UserName;
                }
            }
        }
    } 
}
