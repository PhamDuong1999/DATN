using APP.REPOSITORY;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using APP.MODELS;
using Portal.Utils;
using System.Linq;

namespace APP.MANAGER
{
    public interface ITemporaryBillManager
    {
        Task Create(TemporaryBill inputModel);
        Task Update(TemporaryBill inputModel);
        Task Delete(long id);
        //Task<List<TemporaryBill>> Get_List(string name);
        Task<TemporaryBill> Find_By_Id(long id);
        Task Update_Status(TemporaryBill inputModel);
        Task Sent_KTV(TemporaryBill inputModel);
        Task<List<TemporaryBill_Service>> Get_List_TemporaryBill_Service(long id);
        Task<List<TemporaryBill_Accesary>> Get_List_TemporaryBill_Accesary(long id);
        Task<List<TemporaryBill>> Get_List_Bill(string time, byte status);
        Task<List<TemporaryBill>> Get_List_Bill_KTV(string time,long ktvId);
        Task<List<TemporaryBill>> Get_List_Bill_Month(string time);
        Task<List<TemporaryBill>> Get_List_Bill_Date(string time);
    }
    public class TemporaryBillManager : ITemporaryBillManager
    {
        private readonly IUnitOfWork _unitOfWork;
        public TemporaryBillManager(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<TemporaryBill>> Get_List_Bill(string time, byte status)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBillRepository.FindBy(x => (x.Status == status) && (x.TimeIn.Date.ToString() == time || string.IsNullOrEmpty(time))))
                    .OrderByDescending(x=>x.TimeIn).ToList();
                if(data != null)
                {
                    foreach (var i in data)
                    {
                        var cus = await _unitOfWork.CustomersRepository.Get(c => c.Id == i.CustomerId);
                        i.CustomerName = cus.Name;
                        i.CustomerPhone = cus.Phone;
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TemporaryBill>> Get_List_Bill_KTV(string time,long ktvId)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBillRepository.FindBy(x =>(x.Status != (byte)BillStatus.TemporaryLT) && (x.UpdatedBy == ktvId) && (x.TimeIn.Date.ToString() == time || string.IsNullOrEmpty(time))))
                    .OrderByDescending(x => x.TimeIn).ToList();
                if (data != null)
                {
                    foreach (var i in data)
                    {
                        var cus = await _unitOfWork.CustomersRepository.Get(c => c.Id == i.CustomerId);
                        i.CustomerName = cus.Name;
                        i.CustomerPhone = cus.Phone;
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TemporaryBill>> Get_List_Bill_Month(string time)
        {
            try
            {
                var time1 = time.Split("-");
                var data = (await _unitOfWork.TemporaryBillRepository.FindBy(x => (x.Status == (byte)BillStatus.Bill) 
                                                                            && ((x.TimeOut.Value.Month == int.Parse(time1[1]) && x.TimeOut.Value.Year == int.Parse(time1[0]))
                                                                            || string.IsNullOrEmpty(time))))
                    .OrderByDescending(x => x.TimeIn).ToList();
                if (data != null)
                {
                    foreach (var i in data)
                    {
                        var cus = await _unitOfWork.CustomersRepository.Get(c => c.Id == i.CustomerId);
                        i.CustomerName = cus.Name;
                        i.CustomerPhone = cus.Phone;
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TemporaryBill>> Get_List_Bill_Date(string time)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBillRepository.FindBy(x => (x.Status == (byte)BillStatus.Bill) && (x.TimeOut.Value.Date.ToString() == time || string.IsNullOrEmpty(time))))
                    .OrderByDescending(x => x.TimeIn).ToList();
                if (data != null)
                {
                    foreach (var i in data)
                    {
                        var cus = await _unitOfWork.CustomersRepository.Get(c => c.Id == i.CustomerId);
                        i.CustomerName = cus.Name;
                        i.CustomerPhone = cus.Phone;
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TemporaryBill_Service>> Get_List_TemporaryBill_Service(long id)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBill_ServiceRepository.FindBy(c => c.TemporaryBillId == id)).OrderBy(c=>c.Id).ToList();
                return data;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<List<TemporaryBill_Accesary>> Get_List_TemporaryBill_Accesary(long id)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBill_AccesaryRepository.FindBy(c => c.TemporaryBillId == id)).OrderBy(c => c.Id).ToList();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Create(TemporaryBill inputModel)
        {
            try
            {
                var data = await _unitOfWork.TemporaryBillRepository.Add(inputModel);
                if (inputModel.ListBill_Services != null)
                {
                    await CreateBill_Service(data, inputModel.ListBill_Services);
                }
                if (inputModel.ListBill_Accessories != null)
                {
                    await CreateBill_Accessories(data, inputModel.ListBill_Accessories);
                }
                var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                motorLift.Status = (byte)MotorLiftEnum.Acting;
                await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                if(inputModel.UpdatedBy > 0)
                {
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Acting;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                }
                await _unitOfWork.SaveChange();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private async Task CreateBill_Service(TemporaryBill inputModel,List<TemporaryBill_Service> list)
        {
            try
            {
                foreach (var item in list)
                {
                    item.TemporaryBillId = inputModel.Id;
                    await _unitOfWork.TemporaryBill_ServiceRepository.Add(item);
                    await _unitOfWork.SaveChange();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        private async Task CreateBill_Accessories(TemporaryBill inputModel, List<TemporaryBill_Accesary> list)
        {
            try
            {
                foreach (var item in list)
                {
                    item.TemporaryBillId = inputModel.Id;
                    await _unitOfWork.TemporaryBill_AccesaryRepository.Add(item);
                    if(inputModel.Status == (int)BillStatus.TemporaryTN)
                    {
                        var acc = await _unitOfWork.AccessoriesRepository.Get(c => c.Id == item.AccesaryId);
                        acc.Quantity = acc.Quantity - item.Quantity;
                        await _unitOfWork.AccessoriesRepository.Update(acc);
                    }
                    await _unitOfWork.SaveChange();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task DeleteBill_Service(long billId)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBill_ServiceRepository.FindBy(c => c.TemporaryBillId == billId)).ToList();
                foreach (var item in data)
                {
                    await _unitOfWork.TemporaryBill_ServiceRepository.Delete(item);
                    await _unitOfWork.SaveChange();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task DeleteBill_Accessories(long billId)
        {
            try
            {
                var data = (await _unitOfWork.TemporaryBill_AccesaryRepository.FindBy(c => c.TemporaryBillId == billId)).ToList();
                foreach (var item in data)
                {
                    await _unitOfWork.TemporaryBill_AccesaryRepository.Delete(item);
                    await _unitOfWork.SaveChange();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Update(TemporaryBill inputModel)
        {
            try
            {
                await _unitOfWork.TemporaryBillRepository.Update(inputModel);
                if (inputModel.ListBill_Services != null)
                {
                    await DeleteBill_Service(inputModel.Id);
                    await CreateBill_Service(inputModel, inputModel.ListBill_Services);
                }
                else
                {
                    await DeleteBill_Service(inputModel.Id);
                }
                if (inputModel.ListBill_Accessories != null)
                {
                    await DeleteBill_Accessories(inputModel.Id);
                    await CreateBill_Accessories(inputModel, inputModel.ListBill_Accessories);
                }
                else
                {
                    await DeleteBill_Accessories(inputModel.Id);
                }
                if(inputModel.Status == (byte)BillStatus.TemporaryLT)
                {
                    var oldBill = await _unitOfWork.TemporaryBillRepository.Get(c => c.Id == inputModel.Id);
                    if(inputModel.UpdatedBy != oldBill.UpdatedBy)
                    {
                        var oldktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == oldBill.UpdatedBy);
                        oldktv.StatusActing = (byte)AccountStatusEnum.Active;
                        await _unitOfWork.AccountsRepository.Update(oldktv);
                    }
                    if (inputModel.MotorLiftId != oldBill.MotorLiftId)
                    {
                        var oldmotorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == oldBill.MotorLiftId);
                        oldmotorLift.Status = (byte)MotorLiftEnum.Active;
                        await _unitOfWork.MotorLiftsRepository.Update(oldmotorLift);
                    }
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Acting;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Acting;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                }
                if (inputModel.Status == (byte)BillStatus.TemporaryKTV)
                {
                    var oldBill = await _unitOfWork.TemporaryBillRepository.Get(c => c.Id == inputModel.Id);
                    if (inputModel.UpdatedBy != oldBill.UpdatedBy)
                    {
                        var oldktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == oldBill.UpdatedBy);
                        oldktv.StatusActing = (byte)AccountStatusEnum.Active;
                        await _unitOfWork.AccountsRepository.Update(oldktv);
                    }
                    if (inputModel.MotorLiftId != oldBill.MotorLiftId)
                    {
                        var oldmotorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == oldBill.MotorLiftId);
                        oldmotorLift.Status = (byte)MotorLiftEnum.Active;
                        await _unitOfWork.MotorLiftsRepository.Update(oldmotorLift);
                    }
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Acting;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Acting;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                }
                if (inputModel.Status == (byte)BillStatus.TemporaryTN)
                {
                    inputModel.UpdatedTime = DateTime.Now;
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Active;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Active;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                }
                await _unitOfWork.SaveChange();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Delete(long id)
        {
            try
            {
                var inputModel = await Find_By_Id(id);
                var listBill_Sv = (await _unitOfWork.TemporaryBill_ServiceRepository.FindBy(c => c.TemporaryBillId == id)).ToList();
                if(listBill_Sv != null)
                {
                    await DeleteBill_Service(inputModel.Id);
                }
                var listBill_Acc = (await _unitOfWork.TemporaryBill_AccesaryRepository.FindBy(c => c.TemporaryBillId == id)).ToList();
                if (listBill_Sv != null)
                {
                    await DeleteBill_Accessories(inputModel.Id);
                }
                if(inputModel.UpdatedBy > 0)
                {
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Active;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                }
                if(inputModel.MotorLiftId > 0)
                {
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Active;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                }
                await _unitOfWork.TemporaryBillRepository.Delete(inputModel);
                await _unitOfWork.SaveChange();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<TemporaryBill> Find_By_Id(long id)
        {
            try
            {
                var data = await _unitOfWork.TemporaryBillRepository.Get(c => c.Id == id);
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task Sent_KTV(TemporaryBill inputModel)
        {
            try
            {
                await _unitOfWork.TemporaryBillRepository.Update(inputModel);
                var listAccessories = (await _unitOfWork.TemporaryBill_AccesaryRepository.FindBy(c => c.TemporaryBillId == inputModel.Id)).ToList();
                if (listAccessories != null)
                {
                    foreach (var item in listAccessories)
                    {
                        if (inputModel.Status == (int)BillStatus.TemporaryKTV)
                        {
                            var acc = await _unitOfWork.AccessoriesRepository.Get(c => c.Id == item.AccesaryId);
                            acc.Quantity = acc.Quantity + item.Quantity;
                            await _unitOfWork.AccessoriesRepository.Update(acc);
                        }
                    }
                }
                await _unitOfWork.SaveChange();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task Update_Status(TemporaryBill inputModel)
        {
            try
            {
                await _unitOfWork.TemporaryBillRepository.Update(inputModel);
                if (inputModel.Status == (byte)BillStatus.TemporaryLT)
                {
                    var oldBill = await _unitOfWork.TemporaryBillRepository.Get(c => c.Id == inputModel.Id);
                    if (inputModel.UpdatedBy != oldBill.UpdatedBy)
                    {
                        var oldktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == oldBill.UpdatedBy);
                        oldktv.StatusActing = (byte)AccountStatusEnum.Active;
                        await _unitOfWork.AccountsRepository.Update(oldktv);
                    }
                    if (inputModel.MotorLiftId != oldBill.MotorLiftId)
                    {
                        var oldmotorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == oldBill.MotorLiftId);
                        oldmotorLift.Status = (byte)MotorLiftEnum.Active;
                        await _unitOfWork.MotorLiftsRepository.Update(oldmotorLift);
                    }
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Acting;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Acting;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                }
                if (inputModel.Status == (byte)BillStatus.TemporaryKTV)
                {
                    var oldBill = await _unitOfWork.TemporaryBillRepository.Get(c => c.Id == inputModel.Id);
                    if (inputModel.UpdatedBy != oldBill.UpdatedBy)
                    {
                        var oldktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == oldBill.UpdatedBy);
                        oldktv.StatusActing = (byte)AccountStatusEnum.Active;
                        await _unitOfWork.AccountsRepository.Update(oldktv);
                    }
                    if (inputModel.MotorLiftId != oldBill.MotorLiftId)
                    {
                        var oldmotorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == oldBill.MotorLiftId);
                        oldmotorLift.Status = (byte)MotorLiftEnum.Active;
                        await _unitOfWork.MotorLiftsRepository.Update(oldmotorLift);
                    }
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Acting;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Acting;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                }
                if (inputModel.Status == (byte)BillStatus.TemporaryTN)
                {
                    inputModel.UpdatedTime = DateTime.Now;
                    var ktv = await _unitOfWork.AccountsRepository.Get(c => c.Id == inputModel.UpdatedBy);
                    ktv.StatusActing = (byte)AccountStatusEnum.Active;
                    await _unitOfWork.AccountsRepository.Update(ktv);
                    var motorLift = await _unitOfWork.MotorLiftsRepository.Get(c => c.Id == inputModel.MotorLiftId);
                    motorLift.Status = (byte)MotorLiftEnum.Active;
                    await _unitOfWork.MotorLiftsRepository.Update(motorLift);
                    var listAccessories = (await _unitOfWork.TemporaryBill_AccesaryRepository.FindBy(c => c.TemporaryBillId == inputModel.Id)).ToList();
                    if (listAccessories != null)
                    {
                        foreach (var item in listAccessories)
                        {
                            if (inputModel.Status == (int)BillStatus.TemporaryTN)
                            {
                                var acc = await _unitOfWork.AccessoriesRepository.Get(c => c.Id == item.AccesaryId);
                                acc.Quantity = acc.Quantity - item.Quantity;
                                await _unitOfWork.AccessoriesRepository.Update(acc);
                            }
                        }
                    }
                }
                await _unitOfWork.SaveChange();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
