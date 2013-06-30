//
// IOPropertyValue.cpp : implements IO property value classes
//
#include "nsp/IOTypes.h"
#include "nsp/IOPropertyValue.h"
#include "nsp/PropertyValue.h"
#include "nsp/IOAppMessage.h"
#include "nsp/IOChannel.h"
#include "nsp/Contract.h"
#include "nsp/IOChannel.h"
#include "nsp/CIOExceptions.h"
#include "nsp/OutboundValueConverter.h"

using tac::nsp::cio::IOElecTypeValue;
using tac::nsp::cio::EElecType;
using tac::nsp::cio::EIOCompoundValueType;
using tac::nsp::cio::IOCompoundValue;
using tac::nsp::cio::IOChannel;
using tac::nsp::cio::IOAppMessage;
using tac::nsp::cio::EIOChanType;
using tac::nsp::PropertyValue;
using tac::nsp::DataType;
using tac::nsp::UnitId;
using tac::nsp::cio::CIOInternalErrorException;
using tac::nsp::cio::CIOInvalidUpLinkMsgException;
using tac::nsp::cio::OutboundValueConverter;
using tac::nsp::cio::OutboundConvertibleCompoundValue;
using tac::nsp::cio::DigitalOverrideValue;
using tac::nsp::cio::EElecType;


///////////////////////////////////////////////////////////////////////////////
// IOElecTypeValue

bool IOElecTypeValue::IsValid(const PropertyValue& propValue, const IOChannel* pIOChannel) const
{
  bool isValid = false;
  if ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) || propValue.GetDataType() == DataType::Int64)
  {
    Poco::Int32 value = propValue.GetValue().GetAsInteger();
    if (pIOChannel)
    {
      EIOChanType chanType = pIOChannel->GetChannelType();
      switch (chanType)
      {
      case ectUI:
        if (value == EETUnassigned || value == EETVoltage || value == EETDigital || value == EETCounter || 
            value == EETTempF || value == EETTempC || value == EETSupervised || value == EETCurrent )
            isValid = true;
        break;
      case ectVO:
        if (value == EETUnassigned || value == EETVoltage) isValid = true;
        break;
      case ectDI:
        if (value == EETUnassigned || value == EETDigital || value == EETCounter) isValid = true;
        break;
      case ectDO:
        // If the current one is tristate slave, the new one has to be unassigned or the same
        // If the new electrical type is tristate-slave, the current one has to be unassigned or the same
        if (m_value == EETTristateSlave)    
        {
          if (value == EETUnassigned || value == EETTristateSlave) isValid = true;
        }
        else 
        {
          if (value == EETTristateSlave)
          {
            if (m_value == EETUnassigned) isValid = true;
          }
          else
          {
            if (value == EETUnassigned || value == EETDigital || value == EETPulse || value == EETTristate || value == EETTristatePulse) isValid = true;
          }
        }
        break;
#if Tristate_Channel_Type
      case ectTS:
        if (value == EETUnassigned || value == EETDigital || value == EETPulse || value == EETTristate || value == EETTristatePulse ) isValid = true;
        break;
#endif
      case ectCO:
        if (value == EETUnassigned || value == EETCurrent) isValid = true;
        break;
      case ectAO:
        if (value == EETUnassigned || value == EETCurrent || value == EETVoltage) isValid = true;
        break;
      default: 
        if (value == EETUnassigned) isValid = true; 
        break;
      }
    }
    else if (value > EETUnassigned && value <= EETLast) isValid = true;   // No context case
  }
  return isValid;
}

void IOElecTypeValue::Set(const PropertyValue& propValue)
{
  m_value = (EElecType)(propValue.GetValue().GetAsInteger());
}

void IOElecTypeValue::Get(PropertyValue& propValue) const
{
  propValue = PropertyValue(Value(DataType::Integer, (Poco::Int32)m_value), UnitId());
}

Poco::UInt16 IOElecTypeValue::Pack(IOAppMessage& msg) const
{
  Poco::UInt16 pos;
  if (m_value == EETPulse) pos = msg.add((Poco::UInt8)EETDigital);      // Report pulse as digital. Value has extra info needed
  else if (m_value == EETTristatePulse) pos = msg.add((Poco::UInt8)EETTristate);  // Report tristate pulse as tristate. Value has extra info needed
  else pos = msg.add((Poco::UInt8)m_value);
  return pos;
}
///////////////////////////////////////////////////////////////////////////////
// IOCompoundValue

IOCompoundValue::IOCompoundValue() : m_type(EVTNotSet) 
{ 
  ResetValue();
}

IOCompoundValue::IOCompoundValue(const IOCompoundValue& r) : m_type(r.m_type) 
{ 
  memcpy(&m_value, &r.m_value, sizeof(m_value)); 
}



bool IOCompoundValue::operator == (const PropertyValue& propValue) const
{
  bool isEqual = false;
  switch (m_type)
  {
  case EVTNotSet: break;
  case EVTFloat:
    if ((propValue.GetDataType() == DataType::Float || propValue.GetDataType() == DataType::Double) && propValue.GetValue().GetAsDouble() == m_value.f) isEqual = true;
    break;
  case EVTDWORD:
    if ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) || propValue.GetDataType() == DataType::Int64 && propValue.GetValue().GetAsInteger() == m_value.dw) isEqual = true;
    break;
  case EVTWORD:
    if ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) && propValue.GetValue().GetAsInteger() == m_value.w) isEqual = true;
    break;
  case EVTBYTE:
    if ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) && propValue.GetValue().GetAsInteger() == m_value.byte) isEqual = true;
    break;
  case EVTTristate:
    if ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) && propValue.GetValue().GetAsInteger() == m_value.t) isEqual = true;
    break;
  case EVTBoolean:
    if (propValue.GetDataType() == DataType::Boolean && propValue.GetValue().GetAsBoolean() == m_value.b) isEqual = true;
    else if (propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64)
    {
      Poco::Int32 ival = propValue.GetValue().GetAsInteger();
      if ((m_value.b && ival == 1) || (!m_value.b && ival == 0)) isEqual = true;
    }
    break;
  case EVTSupervised:
    if ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) && propValue.GetValue().GetAsInteger() == m_value.sv) isEqual = true;
    break;
  default:
    NSP_ENSURE(false);
    isEqual = false;
  }
  return isEqual;     
}

void IOCompoundValue::Set(const PropertyValue& propValue)
{
  // NOTE AGAIN: THIS METHOD DOESN'T DO VALIDATION. MUST CALL ISVALID BEFORE CALL THIS ONE
  switch (m_type)
  {
  case EVTFloat:    m_value.f = (float)propValue.GetValue().GetAsDouble(); break;
  case EVTDWORD:    m_value.dw = (Poco::UInt32)propValue.GetValue().GetAsInteger(); break;
  case EVTWORD:     m_value.w = (Poco::UInt16)propValue.GetValue().GetAsInteger(); break;
  case EVTBYTE:     m_value.byte = (Poco::UInt8)propValue.GetValue().GetAsInteger(); break;
  case EVTTristate: m_value.t = (ETristateValue)propValue.GetValue().GetAsInteger(); break;
  case EVTBoolean:  m_value.b = propValue.GetValue().GetAsBoolean(); break;
  case EVTSupervised: m_value.sv = (ESupervisedValue)propValue.GetValue().GetAsInteger(); break;
  default: NSP_ENSURE(false); break;
  }
}


void IOCompoundValue::Get(PropertyValue& propValue) const
{
  //* TBD: IPropertyAccess interface uses auto_ptr. Will this cause memory leak or corruption?
  //* TBD: How to use online/error code defined in PropertyValue

#ifdef DEBUG
  TRACE("IOCompoundValue::Get");
#endif

  switch (m_type)
  {
  case EVTFloat:  
    {
      PropertyValue fv(Value(DataType::Float, m_value.f), UnitId());
      propValue = fv;
    }
    break;
  case EVTDWORD:    
    {
      PropertyValue v(Value(DataType::Integer, (Poco::Int32)m_value.dw), UnitId());
      propValue = v;
    } 
    break;
  case EVTWORD:     
    {
      PropertyValue v(Value(DataType::Integer, (Poco::Int32)m_value.w), UnitId());
      propValue = v;
    } 
    break;
  case EVTBYTE:
    {
      PropertyValue v(Value(DataType::Integer, (Poco::Int32)m_value.byte), UnitId());
      propValue = v;
    } 
    break;
  case EVTTristate:
    {
      PropertyValue v(Value(DataType::Integer, (Poco::Int32)m_value.t), UnitId());
      propValue = v;
    }
    break;
  case EVTBoolean:
    {
      PropertyValue bv(Value(DataType::Boolean, m_value.b), UnitId());
      propValue = bv;
    } 
    break;
  case EVTSupervised:
    {
      PropertyValue sv(Value(DataType::Integer, (Poco::Int32)m_value.sv), UnitId::s_NoUnit);
      propValue = sv;
    }
    break;
  default: 
    {
      PropertyValue v(Value(DataType::Integer, 0), UnitId());
      propValue = v;
    }
    break;  
  }
}

bool IOCompoundValue::IsValid(const PropertyValue& propValue, const IOChannel* pIOChannel) const
{
  bool isValid = false;
  switch (m_type)
  {
  case EVTNotSet: break;
  case EVTFloat:
    if (propValue.GetDataType() == DataType::Float || propValue.GetDataType() == DataType::Double) isValid = true;
    break;
  case EVTDWORD:
    if (propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64)
    {
      unsigned long uival = (unsigned long)propValue.GetValue().GetAsInteger();
      if (uival <= 0xFFFFFFFF) isValid = true;
    }
    break;
  case EVTWORD:
    if (propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64)
    {
      unsigned long uival = (unsigned long)propValue.GetValue().GetAsInteger();
      if (uival <= 0xFFFF) isValid = true;
    }
    break;
  case EVTBYTE:
    if (propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64)
    {
      unsigned long uival = (unsigned long)propValue.GetValue().GetAsInteger();
      if (uival <= 0xFF) isValid = true;
    }
    break;
  case EVTTristate:
    if (propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64)
    {
      unsigned long uival = (unsigned long) propValue.GetValue().GetAsInteger();
      if (uival == ETVOff || uival == ETVOn || uival == ETVMinusOn) isValid = true;
    }
    break;
  case EVTBoolean:
    //* TBD: Temporarily allow integer, otherwise NSP station won't display certain values currectly
    if ((propValue.GetDataType() == DataType::Boolean) ||
        ((propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64) && (propValue.GetValue().GetAsInteger() == 0 || propValue.GetValue().GetAsInteger() == 1)))
        isValid = true;
    break;
  case EVTSupervised:
    if (propValue.GetDataType() == DataType::Integer || propValue.GetDataType() == DataType::Int64)
    {
      Poco::Int32 iv = (Poco::Int32)propValue.GetValue().GetAsInteger();
      if (iv >= ESVNotSet && iv <= ESVEncryptedDeviceFailure) isValid = true;
    }
    break;
  default:
    NSP_ENSURE(false);
    isValid = false;
  }
  return isValid;       
}

void IOCompoundValue::ResetValue()
{
  memset(&m_value,0, sizeof(m_value));   
}

void IOCompoundValue::SetType(EIOCompoundValueType type)
{
  if (m_type != type)
  {
    m_type = type;
    ResetValue();
  }
}


///////////////////////////////////////////////////////////////////////////////
// OutboundConvertibleCompoundValue
OutboundConvertibleCompoundValue::OutboundConvertibleCompoundValue() : IOCompoundValue(), m_pConverter(NULL)
{
}

OutboundConvertibleCompoundValue::OutboundConvertibleCompoundValue(const OutboundConvertibleCompoundValue& r)
: IOCompoundValue(r), m_pConverter(r.m_pConverter)
{
}

Poco::UInt16 OutboundConvertibleCompoundValue::Pack(IOAppMessage& msg) const
{
  Poco::UInt16 pos;
  if (m_pConverter) pos = m_pConverter->Pack(*this, msg);
  else throw CIOInternalErrorException(__FILE__, __LINE__);
  return pos;
}


///////////////////////////////////////////////////////////////////////////////
// DigitalOverrideValue
bool DigitalOverrideValue::Unpack(IOAppMessage& msg)
{
  bool changed = false;
  if (m_type == ENotSet) throw CIOInternalErrorException(__FILE__,__LINE__);
  else
  {
    Poco::UInt8 aByte;
    msg.peek(aByte);          // Just peek, OverrideState still needs it
    aByte = aByte & 0x7F;     // Clear the override state
    if (m_type == EDigital)
    {
      // Only take 1 or 0
      if (aByte > 1) throw CIOInvalidUpLinkMsgException(__FILE__,__LINE__);
      if ((m_value.b && !aByte) || (!m_value.b && aByte))
      {
        m_value.b = aByte ? true : false;
        changed = true;
      }
    }
    else    // Tristate
    {
      // Only take 0-3
      if (aByte > 3) throw CIOInvalidUpLinkMsgException(__FILE__,__LINE__);
      if (m_value.byte != aByte)
      {
        m_value.byte = aByte;
        changed = true;
      }
    }
  }
  return changed;
}


void DigitalOverrideValue::Get(PropertyValue& propValue) const
{
  if (m_type == ENotSet) throw CIOInternalErrorException(__FILE__,__LINE__);
  else if (m_type == EDigital)
  {
    PropertyValue v(Value(DataType::Boolean, m_value.b), UnitId());
    propValue = v;
  }
  else
  {
    PropertyValue v(Value(DataType::Integer, (Poco::Int32)m_value.byte), UnitId());
    propValue = v;
  }
}

