using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrialMonitoring.Core.Enums;

namespace IndustrialMonitoring.Core.Entities
{
    public class Equipment
    {
        private const int MaxSensorsPerEquipment = 10;
        private const int DefaultMaintenanceIntervalDays = 90;

        public Equipment(string name, string serialNumber, EquipmentType type, string manufacturer, string model, DateTime installationDate, int areaId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Nome do Equipmaneto é obrigatório", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new ArgumentException("Número de série é obrigatório", nameof(serialNumber));
            }

            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                throw new ArgumentException("Fabricante é obrigatório", nameof(manufacturer));
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException("Modelo é obrigatório", nameof(model));
            }

            if (installationDate > DateTime.UtcNow)
            {
                throw new ArgumentException("Data de instalação não pode ser futura", nameof(installationDate));
            }

            if (areaId <= 0)
            {
                throw new ArgumentException("AreaId deve ser maior que zero", nameof(areaId));
            }

            Name = name;
            SerialNumber = serialNumber.ToUpper();
            Type = type;
            Manufacturer = manufacturer;
            Model = model;
            InstallationDate = installationDate;
            AreaId = areaId;

            Status = EquipmentStatus.Stopped;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string SerialNumber { get; private set; }
        public EquipmentType Type { get; private set; }
        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public EquipmentStatus Status { get; private set; }
        public DateTime InstallationDate { get; private set; }
        public int AreaId { get; set; }
        public Area Area { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        // List of sensors
        private readonly List<Sensor> _sensors = new();
        private readonly List<MaintenanceRecord> _maintenanceRecords = new();

        public IReadOnlyCollection<Sensor> Sensors => _sensors.AsReadOnly();
        public IReadOnlyCollection<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords.AsReadOnly();


        public bool AddSensor(Sensor sensor)
        {
            int MaxSensorsPerEquipment = 10;

            if (sensor == null)
            {
                throw new ArgumentNullException(nameof(sensor));
            }

            if (_sensors.Any(s => s.Id == sensor.Id))
            {
                return false;
            }

            if (Status == EquipmentStatus.Running)
            {
                return false;
            }

            if (_sensors.Count >= MaxSensorsPerEquipment)
            {
                return false;
            }

            _sensors.Add(sensor);
            UpdateTimestamp();

            return true;
        }

        public bool RemoveSensor(Sensor sensor)
        {
            if (sensor == null)
            {
                throw new ArgumentNullException(nameof(sensor));
            }

            if (Status == EquipmentStatus.Running)
            {
                return false;
            }

            UpdateTimestamp();
            _sensors.Remove(sensor);

            return true;
        }

        public void Start()
        {
            if (!CanStart())
            {
                throw new InvalidOperationException("O equipamento não pode ser inicado no estado atual.");
            }

            Status = EquipmentStatus.Running;
            UpdateTimestamp();
        }

        public void Stop()
        {
            if (Status == EquipmentStatus.Maintenance)
            {
                throw new InvalidOperationException("Não é possível parar o equipamento em manutenção");
            }

            Status = EquipmentStatus.Stopped;
            UpdateTimestamp();
        }

        public void SetMaintenance()
        {
            if (Status != EquipmentStatus.Stopped)
            {
                throw new InvalidOperationException("Equipamento deve estar parado para entrar em manutenção");
            }

            Status = EquipmentStatus.Maintenance;
            UpdateTimestamp();

        }

        public void SetFault()
        {
            Status = EquipmentStatus.Fault;
            UpdateTimestamp();
        }

        public int GetOperationalDays()
        {
            if (InstallationDate > DateTime.Now)
            {
                return 0;
            }

            return (DateTime.Today - InstallationDate.Date).Days;
        }

        public DateTime? GetNextMaintenanceDate()
        {
            var lastMaintenance = _maintenanceRecords.Where(m => m.CompletedDate.HasValue)
                                                     .OrderByDescending(m => m.CompletedDate)
                                                     .FirstOrDefault();

            if (lastMaintenance?.CompletedDate != null)
            {
                return lastMaintenance.CompletedDate.Value.AddDays(DefaultMaintenanceIntervalDays);
            }

            return InstallationDate.AddDays(DefaultMaintenanceIntervalDays);
        }

        // State consulting
        public bool IsOperational()
        {
            if (Status == EquipmentStatus.Running)
            {
                return true;
            }

            return false;
        }

        public bool HasActiveSensors()
        {
            return _sensors.Any(s => s.IsActive);
        }

        public bool NeedsMaintenance()
        {
            if (GetNextMaintenanceDate().HasValue && GetNextMaintenanceDate().Value.Date < DateTime.Today)
            {
                return true;
            }

            return false;
        }

        public int GetSensorCount()
        {
            return _sensors.Count();
        }

        public int GetMaintenanceCount()
        {
            return _maintenanceRecords.Count(m => m.CompletedDate.HasValue);
        }

        // Business validation
        public bool CanStart()
        {
            if (Status == EquipmentStatus.Fault || Status == EquipmentStatus.Running)
            {
                return false;
            }

            if (!HasActiveSensors())
            {
                return false;
            }

            if (Area != null && !Area.IsActive)
            {
                return false;
            }

            var NextMaintenanceDate = GetNextMaintenanceDate();
            if (NextMaintenanceDate.HasValue && NextMaintenanceDate.Value.AddDays(-30) < DateTime.Today)
            {
                return false;
            }

            return ValidateBusinessRules();
        }

        public bool CanReceiveMaintenance()
        {
            if (Status != EquipmentStatus.Stopped)
            {
                return false;
            }

            return true;
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        private bool ValidateStatusTransition(EquipmentStatus newStatus)
        {
            return newStatus switch
            {
                EquipmentStatus.Running => Status == EquipmentStatus.Stopped || Status == EquipmentStatus.Maintenance,
                EquipmentStatus.Stopped => Status != EquipmentStatus.Maintenance,
                EquipmentStatus.Maintenance => Status == EquipmentStatus.Stopped,
                EquipmentStatus.Fault => true,
                _ => false
            };
        }

        public bool ValidateBusinessRules()
        {
            return true;
        }
        
        public override string ToString()
        {
            var areaName = Area?.Name ?? "N/A";
            return $"{Name} ({Status}) - Área: {areaName} - {_sensors.Count} sensores";
        }
    }
}