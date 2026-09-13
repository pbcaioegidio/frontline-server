using Plugin.Core;
using Plugin.Core.Enums;
using Plugin.Core.SQL;
using System;

namespace Plugin.Core.Utility
{
    /// <summary>
    /// Arma Especial 2 (item 1600109 / 1600110).
    ///
    /// O cadeado NÃO é resolvível pelo servidor neste build de client: o
    /// ItemGroup.dat do client lista os itens de efeito 16000xx e pula de
    /// 1600080 para 1600163 — sem entrada para 1600109/1600110. Sem isso o
    /// client não resolve o good do BuyExtend e o Aviso abre vazio (0 dias /
    /// 0 Gold), sem enviar EXTEND_REQ. Comparação: o ExtraGrenade (1600035)
    /// está no ItemGroup.dat e tem os goods 170003501..04 gravados no
    /// Shop.dat local, por isso funciona sem o servidor mandar nada.
    ///
    /// Enquanto o client não for corrigido, os goods ficam invisíveis para
    /// não poluir a loja com cards sem PEF (risco de "Please Wait").
    /// </summary>
    public static class InventoryUnlocks
    {
        public const int Throwing2ItemId = 1600109;
        public const int Throwing2ItemIdAlt = 1600110;

        public static bool IsThrow2UnlockGood(int goodId)
        {
            int baseId = goodId / 100;
            return baseId == Throwing2ItemId || baseId == Throwing2ItemIdAlt;
        }

        /// <summary>
        /// Devolve o catálogo ao padrão do ExtraGrenade (invisível) e remove o
        /// cupom 1700109 que tinha sido criado para tentar abrir o cadeado.
        /// </summary>
        public static void ResetThrow2ShopCatalog()
        {
            try
            {
                using (var conn = ConnectionSQL.GetInstance().Conn())
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"
UPDATE system_shop
SET item_visible = false,
    item_consume = 2,
    ""Item_count_list"" = '1,1,1,1',
    variant_code_list = '04,06,08,12',
    price_cash_list = '250,0,1200,4000',
    price_gold_list = '0,1,0,0'
WHERE item_id IN (1600109, 1600110);

DELETE FROM system_shop_effects WHERE coupon_id = 1700109;
";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CLogger.Print($"InventoryUnlocks.ResetThrow2ShopCatalog: {ex.Message}", LoggerType.Warning);
            }
        }
    }
}
