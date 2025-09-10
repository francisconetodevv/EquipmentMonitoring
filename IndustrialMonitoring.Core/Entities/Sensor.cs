using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrialMonitoring.Core.Enums;

namespace IndustrialMonitoring.Core.Entities
{
    public class Sensor
    {
        private const int DefaultCalibrationIntervalDays = 180;
        private const int MaxPrecision = 4;
        private const int MinPrecision = 0;

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

        public bool SetAlarmLimits(decimal? highAlarm, decimal? lowAlarm, decimal? highWarning, decimal? lowWarning)
        {
            if (highAlarm.HasValue && (highAlarm.Value < MinValue || highAlarm.Value > MaxValue))
            {
                return false;
            }

            if (lowAlarm.HasValue && (lowAlarm.Value < MinValue || lowAlarm.Value > MaxValue))
            {
                return false;
            }

            if (lowAlarm.HasValue && highAlarm.HasValue && lowAlarm.Value >= highAlarm.Value)
            {
                return false;
            }

            if (highWarning.HasValue && highAlarm.HasValue && highWarning.Value >= highAlarm.Value)
            {
                return false;
            }

            if (lowWarning.HasValue && lowAlarm.HasValue && lowWarning.Value <= lowAlarm.Value)
            {
                return false;
            }

            if (highWarning.HasValue && (highWarning < MinValue || highWarning > MaxValue))
            {
                return false;
            }

            if (lowWarning.HasValue && (lowWarning < MinValue || lowWarning > MaxValue))
            {
                return false;
            }

            if (!IsActive)
            {
                return false;
            }

            HighWarningLimit = highWarning;
            LowWarningLimit = lowWarning;
            HighAlarmLimit = highAlarm;
            LowAlarmLimit = lowAlarm;

            UpdateTimestamp();
            return true;
        }

        public bool AddReading(decimal value, DateTime timestamp)
        {
            if (!CanReceiveReading())
            {
                return false;
            }

            if (timestamp > DateTime.UtcNow)
            {
                return false;
            }

            if (LastReadingDate.HasValue && timestamp <= LastReadingDate.Value)
            {
                return false;
            }

            ValidateValueRange(value);

            var roundedValue = RoundToPrecision(value);
            var reading = new SensorReading(roundedValue, timestamp, Id);

            _sensorReadings.Add(reading);

            LastReadingValue = roundedValue;
            LastReadingDate = timestamp;

            CreateAlarmIfNeeded(roundedValue, timestamp);
            Status = DetermineStatusByValue(roundedValue);

            UpdateTimestamp();
            return true;
        }

        public SensorReading GetLatestReading() {
            return _sensorReadings.OrderByDescending(r => r.Timestamp).FirstOrDefault();
        }

        public IEnumerable<SensorReading> GetReadingsByPeriod(DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
                throw new ArgumentException("Data inicial deve ser menor ou igual à data final");

            return _sensorReadings
                .Where(r => r.Timestamp >= startDate && r.Timestamp <= endDate)
                .OrderBy(r => r.Timestamp)
                .ToList();
        }

        public void Activate()
        {
            if (Status == SensorStatus.Fault)
            {
                throw new InvalidOperationException("Sensor com falha não pode ser ativado");
            }

            IsActive = true;
            Status = SensorStatus.Normal;
            UpdateTimestamp();
        }

        public void Deactivate()
        {
            IsActive = false;
            Status = SensorStatus.Offline;
            UpdateTimestamp();
        }

        public void SetFault(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("Motivo da falha é obrigatório", nameof(reason));
            }

            IsActive = false;
            Status = SensorStatus.Fault;

            var alarm = new Alarm($"Sensor em falha: {reason}", AlarmSeverity.High, DateTime.UtcNow, Id);
            _alarms.Add(alarm);

            UpdateTimestamp();
        }

        public void StartCalibration()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("O sensor deve está ativo para calibração");
            }

            Status = SensorStatus.Calibrating;
            UpdateTimestamp();
        }

        public void FinishCalibration()
        {
            if (Status != SensorStatus.Calibrating)
            {
                throw new InvalidOperationException("Sensor deve estar em calibração para finalizar");
            }

            Status = SensorStatus.Normal;
            CalibrationDate = DateTime.UtcNow;
            UpdateTimestamp();
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
        private void UpdateTimestamp()
        {

        }

        private decimal RoundToPrecision(decimal value)
        {

        }

        private SensorType DetermineStatusByValue(decimal value)
        {

        }

        private void CreateAlarmIfNeeded(decimal value, DateTime timestamp)
        {

        }

        private void ValidateValueRange(decimal value)
        {

        }

        private bool IsWithinAlarmLimits(decimal value)
        {

        }

        public override string ToString()
        {
            return $"{}"
        }
    }
}