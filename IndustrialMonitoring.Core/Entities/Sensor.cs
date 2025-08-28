using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrialMonitoring.Core.Enums;

namespace IndustrialMonitoring.Core.Entities
{
    public class Sensor
    {
        public Sensor(string name, string description, SensorType type, string unit, decimal minValue, decimal maxValue, int precision, int equipmentId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Nome do sensor é obrigatório", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Descrição do sensor é obrigatória", nameof(description));
            }

            if (string.IsNullOrWhiteSpace(unit))
            {
                throw new ArgumentException("Unidade do sensor é obrigatória", nameof(unit));
            }

            if (minValue > maxValue)
            {
                throw new ArgumentException("O valor mínimo é menor que o máximo", nameof(minValue));
            }

            if (precision < -1 || precision > 5)
            {
                throw new ArgumentException("A precisão deve está entre 0 e 4", nameof(precision));
            }

            if (equipmentId <= 0)
            {
                throw new ArgumentException("EquipmentId deve ser maior que zero", nameof(equipmentId));
            }

            IsActive = true;
            Status = SensorStatus.Normal;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            LastReadingValue = null;
            LastReadingDate = null;
            CalibrationDate = null;
            HighAlarmLimit = null;
            LowAlarmLimit = null;
            HighWarningLimit = null;
            LowWarningLimit = null;
        }
        
        public int Id { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SensorType Type { get; set; }
        public string Unit { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public int Precision { get; set; }
        public bool IsActive { get; set; }
        public SensorStatus Status { get; set; }
        public decimal? LastReadingValue { get; set; }
        public DateTime? LastReadingDate { get; set; }
        public decimal? HighAlarmLimit { get; set; }
        public decimal? LowAlarmLimit { get; set; }
        public decimal? HighWarningLimit { get; set; }
        public decimal? LowWarningLimit { get; set; }
        public int EquipmentId { get; set; }
        public Equipment Equipment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CalibrationDate { get; set; }

        private readonly List<SensorReading> _sensorReadings = new();
        private readonly List<Alarm> _alarms = new();

        public IReadOnlyCollection<SensorReading> SensorReadings => _sensorReadings.AsReadOnly();
        public IReadOnlyCollection<Alarm> Alarm => _alarms.AsReadOnly();


    }
}