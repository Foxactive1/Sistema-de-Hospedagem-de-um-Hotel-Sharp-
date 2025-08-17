using System;
using System.Collections.Generic;
using System.Linq;

namespace SistemaHotel
{
    // Enum para tipos de quarto
    public enum TipoQuarto
    {
        Standard,
        Luxo,
        Suite
    }

    // Enum para status da reserva
    public enum StatusReserva
    {
        Ativa,
        CheckIn,
        CheckOut,
        Cancelada
    }

    // Classe Pessoa
    public class Pessoa
    {
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }

        public Pessoa(string nome, string cpf, string email = "", string telefone = "")
        {
            Nome = nome;
            CPF = cpf;
            Email = email;
            Telefone = telefone;
        }

        public override string ToString()
        {
            return $"Nome: {Nome}, CPF: {CPF}";
        }
    }

    // Classe Quarto
    public class Quarto
    {
        public int Numero { get; set; }
        public TipoQuarto Tipo { get; set; }
        public decimal PrecoDiaria { get; set; }
        public bool Disponivel { get; set; }

        public Quarto(int numero, TipoQuarto tipo, decimal precoDiaria)
        {
            Numero = numero;
            Tipo = tipo;
            PrecoDiaria = precoDiaria;
            Disponivel = true;
        }

        public override string ToString()
        {
            string status = Disponivel ? "Disponível" : "Ocupado";
            return $"Quarto {Numero} - {Tipo} - R$ {PrecoDiaria:F2}/dia - {status}";
        }
    }

    // Classe Reserva
    public class Reserva
    {
        public int Id { get; set; }
        public Pessoa Hospede { get; set; }
        public List<Quarto> Quartos { get; set; }
        public DateTime DataCheckIn { get; set; }
        public DateTime DataCheckOut { get; set; }
        public StatusReserva Status { get; set; }
        public decimal ValorTotal { get; private set; }

        public Reserva(int id, Pessoa hospede, List<Quarto> quartos, DateTime checkIn, DateTime checkOut)
        {
            Id = id;
            Hospede = hospede;
            Quartos = new List<Quarto>(quartos);
            DataCheckIn = checkIn;
            DataCheckOut = checkOut;
            Status = StatusReserva.Ativa;
            CalcularValorTotal();
        }

        private void CalcularValorTotal()
        {
            int dias = (DataCheckOut - DataCheckIn).Days;
            if (dias <= 0) dias = 1; // Mínimo 1 dia

            ValorTotal = Quartos.Sum(q => q.PrecoDiaria) * dias;
        }

        public void AtualizarStatus(StatusReserva novoStatus)
        {
            Status = novoStatus;
            
            // Atualiza disponibilidade dos quartos
            foreach (var quarto in Quartos)
            {
                if (novoStatus == StatusReserva.CheckIn)
                    quarto.Disponivel = false;
                else if (novoStatus == StatusReserva.CheckOut || novoStatus == StatusReserva.Cancelada)
                    quarto.Disponivel = true;
            }
        }

        public override string ToString()
        {
            return $"Reserva #{Id} - {Hospede.Nome} - {Quartos.Count} quarto(s) - {Status} - Total: R$ {ValorTotal:F2}";
        }
    }

    // Classe principal do Hotel
    public class Hotel
    {
        private string nome;
        private List<Quarto> quartos;
        private List<Reserva> reservas;
        private int proximoIdReserva;

        public Hotel(string nome)
        {
            this.nome = nome;
            quartos = new List<Quarto>();
            reservas = new List<Reserva>();
            proximoIdReserva = 1;
            
            // Inicializar quartos padrão
            InicializarQuartos();
        }

        private void InicializarQuartos()
        {
            // Quartos Standard (101-120)
            for (int i = 101; i <= 120; i++)
            {
            }

            // Quartos Luxo (201-210)
            for (int i = 201; i <= 210; i++)
            {
                quartos.Add(new Quarto(i, TipoQuarto.Luxo, 300.00m));
            }

            // Suítes (301-305)
            for (int i = 301; i <= 305; i++)
            {
                quartos.Add(new Quarto(i, TipoQuarto.Suite, 500.00m));
            }
        }

        public List<Quarto> ListarQuartosDisponiveis(TipoQuarto? tipo = null)
        {
            var quartosDisponiveis = quartos.Where(q => q.Disponivel);
            
            if (tipo.HasValue)
                quartosDisponiveis = quartosDisponiveis.Where(q => q.Tipo == tipo.Value);
            
            return quartosDisponiveis.ToList();
        }

        public Quarto BuscarQuarto(int numero)
        {
            return quartos.FirstOrDefault(q => q.Numero == numero);
        }

        public Reserva CriarReserva(Pessoa hospede, List<int> numerosQuartos, DateTime checkIn, DateTime checkOut)
        {
            // Validações
            if (checkIn >= checkOut)
                throw new ArgumentException("Data de check-in deve ser anterior ao check-out");

            if (checkIn < DateTime.Today)
                throw new ArgumentException("Data de check-in não pode ser no passado");

            // Buscar quartos solicitados
            var quartosReserva = new List<Quarto>();
            foreach (int numero in numerosQuartos)
            {
                var quarto = BuscarQuarto(numero);
                if (quarto == null)
                    throw new ArgumentException($"Quarto {numero} não encontrado");
                
                if (!quarto.Disponivel)
                    throw new ArgumentException($"Quarto {numero} não está disponível");
                
                quartosReserva.Add(quarto);
            }

            // Criar reserva
            var reserva = new Reserva(proximoIdReserva++, hospede, quartosReserva, checkIn, checkOut);
            
            // Marcar quartos como indisponíveis
            foreach (var quarto in quartosReserva)
            {
                quarto.Disponivel = false;
            }

            reservas.Add(reserva);
            return reserva;
        }

        public void FazerCheckIn(int idReserva)
        {
            var reserva = reservas.FirstOrDefault(r => r.Id == idReserva);
            if (reserva == null)
                throw new ArgumentException("Reserva não encontrada");

            if (reserva.Status != StatusReserva.Ativa)
                throw new InvalidOperationException("Reserva deve estar ativa para fazer check-in");

            if (DateTime.Today < reserva.DataCheckIn.Date)
                throw new InvalidOperationException("Check-in só pode ser feito a partir da data da reserva");

            reserva.AtualizarStatus(StatusReserva.CheckIn);
            Console.WriteLine($"Check-in realizado com sucesso para a reserva #{idReserva}");
        }

        public void FazerCheckOut(int idReserva)
        {
            var reserva = reservas.FirstOrDefault(r => r.Id == idReserva);
            if (reserva == null)
                throw new ArgumentException("Reserva não encontrada");

            if (reserva.Status != StatusReserva.CheckIn)
                throw new InvalidOperationException("Check-out só pode ser feito após check-in");

            reserva.AtualizarStatus(StatusReserva.CheckOut);
            Console.WriteLine($"Check-out realizado com sucesso para a reserva #{idReserva}");
            Console.WriteLine($"Valor total da estadia: R$ {reserva.ValorTotal:F2}");
        }

        public void CancelarReserva(int idReserva)
        {
            var reserva = reservas.FirstOrDefault(r => r.Id == idReserva);
            if (reserva == null)
                throw new ArgumentException("Reserva não encontrada");

            if (reserva.Status == StatusReserva.CheckOut)
                throw new InvalidOperationException("Não é possível cancelar reserva já finalizada");

            reserva.AtualizarStatus(StatusReserva.Cancelada);
            Console.WriteLine($"Reserva #{idReserva} cancelada com sucesso");
        }

        public List<Reserva> ListarReservas(StatusReserva? status = null)
        {
            if (status.HasValue)
                return reservas.Where(r => r.Status == status.Value).ToList();
            
            return new List<Reserva>(reservas);
        }

        public void ExibirRelatorioOcupacao()
        {
            Console.WriteLine($"\n=== RELATÓRIO DE OCUPAÇÃO - {nome} ===");
            Console.WriteLine($"Data: {DateTime.Now:dd/MM/yyyy HH:mm}");
            
            var quartosOcupados = quartos.Count(q => !q.Disponivel);
            var totalQuartos = quartos.Count;
            var taxaOcupacao = (double)quartosOcupados / totalQuartos * 100;
            
            Console.WriteLine($"Total de quartos: {totalQuartos}");
            Console.WriteLine($"Quartos ocupados: {quartosOcupados}");
            Console.WriteLine($"Quartos disponíveis: {totalQuartos - quartosOcupados}");
            Console.WriteLine($"Taxa de ocupação: {taxaOcupacao:F1}%");

            // Ocupação por tipo
            foreach (TipoQuarto tipo in Enum.GetValues<TipoQuarto>())
            {
                var quartosDoTipo = quartos.Where(q => q.Tipo == tipo);
                var ocupados = quartosDoTipo.Count(q => !q.Disponivel);
                var total = quartosDoTipo.Count();
                Console.WriteLine($"{tipo}: {ocupados}/{total} ocupados");
            }
        }
    }

    // Classe para demonstração do sistema
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SISTEMA DE HOSPEDAGEM - HOTEL ===\n");
            
            // Criar hotel
            var hotel = new Hotel("Hotel Paradise");

            try
            {
                // Criar hóspedes
                var hospede1 = new Pessoa("João Silva", "123.456.789-00", "joao@email.com", "(11) 99999-1111");
                var hospede2 = new Pessoa("Maria Santos", "987.654.321-00", "maria@email.com", "(11) 88888-2222");

                // Listar quartos disponíveis
                Console.WriteLine("QUARTOS DISPONÍVEIS:");
                var quartosDisponiveis = hotel.ListarQuartosDisponiveis();
                foreach (var quarto in quartosDisponiveis.Take(5)) // Mostra apenas os primeiros 5
                {
                    Console.WriteLine(quarto);
                }
                Console.WriteLine($"... e mais {quartosDisponiveis.Count - 5} quartos disponíveis\n");

                // Fazer reservas
                Console.WriteLine("FAZENDO RESERVAS:");
                var reserva1 = hotel.CriarReserva(
                    hospede1, 
                    new List<int> { 101, 102 }, 
                    DateTime.Today.AddDays(1), 
                    DateTime.Today.AddDays(4)
                );
                Console.WriteLine($"Reserva criada: {reserva1}");

                var reserva2 = hotel.CriarReserva(
                    hospede2,
                    new List<int> { 301 },
                    DateTime.Today.AddDays(2),
                    DateTime.Today.AddDays(5)
                );
                Console.WriteLine($"Reserva criada: {reserva2}\n");

                // Simular check-in (ajustar data para hoje para demonstração)
                reserva1 = hotel.CriarReserva(
                    hospede1,
                    new List<int> { 201 },
                    DateTime.Today,
                    DateTime.Today.AddDays(3)
                );
                
                Console.WriteLine("FAZENDO CHECK-IN:");
                hotel.FazerCheckIn(reserva1.Id);
                Console.WriteLine();

                // Listar reservas ativas
                Console.WriteLine("RESERVAS ATIVAS:");
                var reservasAtivas = hotel.ListarReservas(StatusReserva.Ativa);
                foreach (var reserva in reservasAtivas)
                {
                    Console.WriteLine(reserva);
                }
                Console.WriteLine();

                // Exibir relatório de ocupação
                hotel.ExibirRelatorioOcupacao();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }

            Console.WriteLine("\nPressione qualquer tecla para sair...");
            Console.ReadKey();
        }
    }
}