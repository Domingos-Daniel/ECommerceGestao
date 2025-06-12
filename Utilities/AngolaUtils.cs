using System;
using System.Collections.Generic;

namespace ECommerceGestao.Utilities
{
    public static class AngolaUtils
    {
        // Províncias de Angola
        public static List<string> Provincias = new List<string> 
        {
            "Bengo", "Benguela", "Bié", "Cabinda", "Cuando Cubango", "Cuanza Norte", "Cuanza Sul", 
            "Cunene", "Huambo", "Huíla", "Luanda", "Lunda Norte", "Lunda Sul", "Malanje", "Moxico", 
            "Namibe", "Uíge", "Zaire"
        };

        // Bancos comuns em Angola
        public static List<string> Bancos = new List<string> 
        {
            "Banco Angolano de Investimentos (BAI)",
            "Banco de Fomento Angola (BFA)",
            "Banco Económico",
            "Banco de Poupança e Crédito (BPC)",
            "Standard Bank Angola",
            "Banco Millennium Atlântico",
            "Banco BIC",
            "Banco Sol",
            "Banco Caixa Geral Angola"
        };

        // Gera um número de referência de pagamento para Angola
        public static string GerarReferenciaMulticaixa(int orderId)
        {
            // Formato: ENTIDADE + REFERÊNCIA (9 dígitos)
            // Entidade padrão para teste: 00000
            string entidade = "00000";
            // Garantir que a referência tenha 9 dígitos com leading zeros
            string referencia = $"{orderId:D6}{DateTime.Now.ToString("ddd")}".Replace("-", "").Substring(0, 9);
            
            return $"{entidade} {referencia}";
        }
        
        // Converte um valor decimal para o formato de moeda angolana
        public static string FormatarKwanza(decimal valor)
        {
            return $"AOA {valor:N2}".Replace(".", ",");
        }
        
        // Valida um formato de BI angolano (simplificado)
        public static bool ValidarBI(string bi)
        {
            // Formato simplificado: 9 dígitos + 2 letras + 3 dígitos (ex: 123456789LA123)
            if (string.IsNullOrEmpty(bi) || bi.Length != 14)
                return false;
                
            // Verificar se os primeiros 9 caracteres são dígitos
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(bi[i]))
                    return false;
            }
            
            // Verificar se os caracteres 10 e 11 são letras maiúsculas
            if (!char.IsUpper(bi[9]) || !char.IsUpper(bi[10]))
                return false;
                
            // Verificar se os últimos 3 caracteres são dígitos
            for (int i = 11; i < 14; i++)
            {
                if (!char.IsDigit(bi[i]))
                    return false;
            }
            
            return true;
        }
    }
}
