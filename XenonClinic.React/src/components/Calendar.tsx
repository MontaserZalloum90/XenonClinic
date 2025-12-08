import { useState } from 'react';
import { format, startOfMonth, endOfMonth, startOfWeek, endOfWeek, addDays, isSameMonth, isSameDay, addMonths, subMonths } from 'date-fns';
import type { Appointment } from '../types/appointment';
import { StatusBadge } from './ui/StatusBadge';

interface CalendarProps {
  appointments: Appointment[];
  onAppointmentClick: (appointment: Appointment) => void;
  onDateClick: (date: Date) => void;
}

export const Calendar = ({ appointments, onAppointmentClick, onDateClick }: CalendarProps) => {
  const [currentMonth, setCurrentMonth] = useState(new Date());

  const renderHeader = () => {
    return (
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-xl font-bold text-gray-900">
          {format(currentMonth, 'MMMM yyyy')}
        </h2>
        <div className="flex gap-2">
          <button
            onClick={() => setCurrentMonth(subMonths(currentMonth, 1))}
            className="p-2 hover:bg-gray-100 rounded-md transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 19l-7-7 7-7" />
            </svg>
          </button>
          <button
            onClick={() => setCurrentMonth(new Date())}
            className="px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
          >
            Today
          </button>
          <button
            onClick={() => setCurrentMonth(addMonths(currentMonth, 1))}
            className="p-2 hover:bg-gray-100 rounded-md transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5l7 7-7 7" />
            </svg>
          </button>
        </div>
      </div>
    );
  };

  const renderDaysOfWeek = () => {
    const days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    return (
      <div className="grid grid-cols-7 mb-2">
        {days.map((day) => (
          <div key={day} className="text-center py-2 text-sm font-semibold text-gray-600">
            {day}
          </div>
        ))}
      </div>
    );
  };

  const getAppointmentsForDay = (day: Date) => {
    return appointments.filter((appointment) =>
      isSameDay(new Date(appointment.startTime), day)
    );
  };

  const renderCells = () => {
    const monthStart = startOfMonth(currentMonth);
    const monthEnd = endOfMonth(monthStart);
    const startDate = startOfWeek(monthStart);
    const endDate = endOfWeek(monthEnd);

    const rows = [];
    let days = [];
    let day = startDate;

    while (day <= endDate) {
      for (let i = 0; i < 7; i++) {
        const dayAppointments = getAppointmentsForDay(day);
        const isCurrentMonth = isSameMonth(day, monthStart);
        const isToday = isSameDay(day, new Date());
        const dayClone = day;

        days.push(
          <div
            key={day.toString()}
            className={`min-h-[120px] border border-gray-200 p-2 ${
              isCurrentMonth ? 'bg-white' : 'bg-gray-50'
            } ${isToday ? 'ring-2 ring-primary-500' : ''} hover:bg-gray-50 transition-colors cursor-pointer`}
            onClick={() => onDateClick(dayClone)}
          >
            <div className={`text-sm font-medium mb-1 ${
              isCurrentMonth ? 'text-gray-900' : 'text-gray-400'
            } ${isToday ? 'text-primary-600 font-bold' : ''}`}>
              {format(day, 'd')}
            </div>
            <div className="space-y-1">
              {dayAppointments.slice(0, 3).map((appointment) => (
                <div
                  key={appointment.id}
                  onClick={(e) => {
                    e.stopPropagation();
                    onAppointmentClick(appointment);
                  }}
                  className="text-xs p-1 rounded bg-primary-50 hover:bg-primary-100 transition-colors cursor-pointer border-l-2 border-primary-500"
                >
                  <div className="font-medium text-gray-900 truncate">
                    {format(new Date(appointment.startTime), 'HH:mm')} - {appointment.patient?.fullNameEn || 'Unknown'}
                  </div>
                  <div className="flex items-center gap-1 mt-0.5">
                    <StatusBadge status={appointment.status} />
                  </div>
                </div>
              ))}
              {dayAppointments.length > 3 && (
                <div className="text-xs text-gray-500 font-medium pl-1">
                  +{dayAppointments.length - 3} more
                </div>
              )}
            </div>
          </div>
        );
        day = addDays(day, 1);
      }
      rows.push(
        <div key={day.toString()} className="grid grid-cols-7">
          {days}
        </div>
      );
      days = [];
    }
    return <div>{rows}</div>;
  };

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="p-4">
        {renderHeader()}
        {renderDaysOfWeek()}
        {renderCells()}
      </div>
    </div>
  );
};
