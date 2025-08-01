using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameData : MonoBehaviour
    {
        public static GameData Instance { get; private set; }

        [Header("JSONs de Dados")]
        public ListaFuncionarios funcionarios;
        public ListaConsumiveis consumiveis;
        public ListaFuncionariosConsumiveis funcionariosConsumiveis;
        public ListaRanking ranking;

        [Header("Referência para busca de personagens")]
        public GameObject listaPersonagens;

        private Dictionary<int, Funcionarios> funcionarioPorId = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            IniciarVinculos();
            GerarConsumiveis();
        }

        private void IniciarVinculos()
        {
            funcionarioPorId.Clear();

            foreach (var func in funcionarios.funcionarios)
            {
                funcionarioPorId[func.id] = func;

                Transform children = listaPersonagens.transform.Find(func.id.ToString());
                if (children != null)
                {
                    func.personagem3D = children.gameObject;
                    Debug.Log($"Funcionario {func.nome} vinculado ao objeto {children.name}");
                }
                else
                {
                    Debug.LogWarning($"Objeto com o nome {func.id} não encontrado");
                }

                foreach (var consumo in funcionariosConsumiveis.funcionarioConsumiveis)
                {
                    if (consumo.idFuncionario == func.id)
                    {
                        var cons = consumiveis.consumiveis.Find(c => c.id == consumo.idConsumivel);
                        string nome = cons != null ? cons.nome : "Desconhecido";
                        Debug.Log($"{func.nome} tem {cons.qtd} de {nome}");
                    }
                }
            }
        }

        public Funcionarios GetFuncionario(int id)
        {
            funcionarioPorId.TryGetValue(id, out var func);
            return func;
        }

        public GameObject GetPersonagem(int id)
        {
            var funcionario = GetFuncionario(id);
            return funcionario?.personagem3D;
        }

        public List<FuncionarioConsumivel> GetConsFunc(int idFunc)
        {
            return funcionariosConsumiveis.funcionarioConsumiveis.FindAll(f => f.idFuncionario == idFunc);
        }

        public int GetPOntosDoJogador(string nome)
        {
            var entry = ranking.ranking.Find(p => p.nome == nome);
            return entry != null ? entry.pontos : 0;
        }

        public void GerarConsumiveis()
        {
            funcionariosConsumiveis.funcionarioConsumiveis.Clear();

            foreach (var func in funcionarios.funcionarios)
            {
                int qtdTipos = Random.Range(1, 8);
                List<int> usados = new();

                for (int i = 0; i < qtdTipos; i++)
                {
                    Consumivel item;
                    do
                    {
                        item = consumiveis.consumiveis[Random.Range(0, consumiveis.consumiveis.Count)];
                    } while (usados.Contains(item.id));

                    usados.Add(item.id);
                    int qtd = Random.Range(1, 5);

                    funcionariosConsumiveis.funcionarioConsumiveis.Add(new FuncionarioConsumivel
                    {
                        idFuncionario = func.id,
                        idConsumivel = item.id,
                        qtd = qtd
                    });
                }
            }

            string path = Path.Combine(Application.persistentDataPath, "funcionariosConsumiveis.json");
            string jsonFinal = JsonUtility.ToJson(funcionariosConsumiveis, true);
            File.WriteAllText(path, jsonFinal);

            Debug.Log("Dados carregados ao JSON.");
        }

        private Dictionary<string, List<int>> restricoesPorFuncao = new()
        {
            { "Limpeza", new List<int> { 9, 10 } },
            { "TI", new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 } },
            { "Dev", new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 } },
            { "Marketing", new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 } }
        };

        public bool VerificarEntregasValidas(int idFuncionario)
        {
            var funcionario = GetFuncionario(idFuncionario);
            if (funcionario == null)
            {
                Debug.LogWarning("Funcionário não encontrado.");
                return false;
            }

            if (!restricoesPorFuncao.TryGetValue(funcionario.funcao, out var permitidos))
            {
                Debug.LogWarning($"Função {funcionario.funcao} não tem restrições definidas.");
                return false;
            }

            var entregas = GetConsFunc(idFuncionario);
            foreach (var entrega in entregas)
            {
                if (!permitidos.Contains(entrega.idConsumivel))
                {
                    var nomeItem = consumiveis.consumiveis.Find(c => c.id == entrega.idConsumivel)?.nome ?? "Desconhecido";
                    Debug.LogWarning($"Item inválido: {nomeItem} ({entrega.idConsumivel}) entregue por {funcionario.nome} ({funcionario.funcao})");
                    return false;
                }
            }

            return true;
        }

    }
}