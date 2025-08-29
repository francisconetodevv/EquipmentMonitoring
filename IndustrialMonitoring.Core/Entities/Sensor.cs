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
        public string Name { get; private set; }
        public string Description { get; private set; }
        public SensorType Type { get; private set; }
        public string Unit { get; private set; }
        public decimal MinValue { get; private set; }
        public decimal MaxValue { get; private set; }
        public int Precision { get; private set; }
        public bool IsActive { get; private set; }
        public SensorStatus Status { get; private set; }
        public decimal? LastReadingValue { get; private set; }
        public DateTime? LastReadingDate { get; private set; }
        public decimal? HighAlarmLimit { get; private set; }
        public decimal? LowAlarmLimit { get; private set; }
        public decimal? HighWarningLimit { get; private set; }
        public decimal? LowWarningLimit { get; private set; }
        public int EquipmentId { get; private set; }
        public Equipment Equipment { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? CalibrationDate { get; private set; }

        private readonly List<SensorReading> _sensorReadings = new();
        private readonly List<Alarm> _alarms = new();

        public IReadOnlyCollection<SensorReading> SensorReadings => _sensorReadings.AsReadOnly();
        public IReadOnlyCollection<Alarm> Alarm => _alarms.AsReadOnly();

        public bool SetAlarmLimits(decimal? highAlarm, decimal? lowAlarm, decimal? highWarning, decimal? highWarning)
        {

        }

        public bool AddReading(decimal value, DateTime timestamp)
        {

        }

        public void Activate()
        {

        }

        public void Deactivate()
        {

        }

        public void SetFault(string reason)
        {

        }

        public void StartCalibration()
        {

        }

        public void FinishCalibration()
        {

        }

        public decimal? GetAverageValue(DateTime startDate, DateTime endDate)
        {

        }

        public decimal? GetMinValue(DateTime startDate, DateTime endDate)
        {

        }

        public decimal? GetMaxValue(DateTime startDate, DateTime endDate)
        {

        }

        public int GetReadingCount()
        {

        }

        public TimeSpan? GetTimeSinceLastReading()
        {

        }

        public bool IsCalibrationDue()
        {

        }

        public bool CanReceiveReading()
        {

        }

        public bool IsInAlarmStage()
        {

        }

        public bool IsInWarningState()
        {

        }

        public bool NeedsCalibration()
        {

        }

        // Aux methods
    }
}