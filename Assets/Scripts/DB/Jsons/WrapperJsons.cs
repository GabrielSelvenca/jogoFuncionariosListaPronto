using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ListaFuncionarios
{
    public List<Funcionarios> funcionarios;
}

[System.Serializable]
public class ListaConsumiveis
{
    public List<Consumivel> consumiveis;
}

[System.Serializable]
public class ListaFuncionariosConsumiveis
{
    public List<FuncionarioConsumivel> funcionarioConsumiveis;
}

[System.Serializable]
public class ListaRanking
{
    public List<PlayerRank> ranking;
}