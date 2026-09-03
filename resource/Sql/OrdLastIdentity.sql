-- OrdLastIdentity : 直前に採番された IDENTITY 値を取る（親 Orders の OrderID）
-- ★ SCOPE_IDENTITY() は「同一スコープ（＝同一バッチ）」でしか有効でないため、
--   INSERT とは別コマンドで実行するここでは NULL になる。
--   @@IDENTITY は同一コネクション内で有効なので、Ｂ層が持つ接続のまま取れる。
--   （Orders にトリガは無いので、トリガ由来の採番を拾う @@IDENTITY の弱点は該当しない）
SELECT
  CAST(@@IDENTITY AS int)
