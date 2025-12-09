import { useMemo } from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  ReferenceLine,
} from 'recharts';
import type { Audiogram, AudiogramDataPoint, AudiogramFrequency } from '../../types/audiology';
import { AUDIOGRAM_FREQUENCIES, HearingLossGrade } from '../../types/audiology';

interface AudiogramChartProps {
  audiogram: Audiogram;
  showBoneConduction?: boolean;
  showNormalRange?: boolean;
  height?: number;
}

// Standard audiogram frequency labels
const FREQUENCY_LABELS: Record<AudiogramFrequency, string> = {
  250: '250',
  500: '500',
  1000: '1K',
  2000: '2K',
  4000: '4K',
  8000: '8K',
};

// Hearing loss grade boundaries (in dB HL)
const GRADE_BOUNDARIES = [
  { threshold: 25, label: 'Normal', color: '#10B981' },
  { threshold: 40, label: 'Mild', color: '#F59E0B' },
  { threshold: 55, label: 'Moderate', color: '#F97316' },
  { threshold: 70, label: 'Mod. Severe', color: '#EF4444' },
  { threshold: 90, label: 'Severe', color: '#DC2626' },
  { threshold: 120, label: 'Profound', color: '#991B1B' },
];

// Transform audiogram data for Recharts
const transformData = (audiogram: Audiogram) => {
  return AUDIOGRAM_FREQUENCIES.map((freq) => {
    const rightAir = audiogram.rightEarAir.find((p) => p.frequency === freq);
    const leftAir = audiogram.leftEarAir.find((p) => p.frequency === freq);
    const rightBone = audiogram.rightEarBone?.find((p) => p.frequency === freq);
    const leftBone = audiogram.leftEarBone?.find((p) => p.frequency === freq);

    return {
      frequency: freq,
      frequencyLabel: FREQUENCY_LABELS[freq],
      rightAir: rightAir?.noResponse ? null : rightAir?.threshold,
      leftAir: leftAir?.noResponse ? null : leftAir?.threshold,
      rightBone: rightBone?.noResponse ? null : rightBone?.threshold,
      leftBone: leftBone?.noResponse ? null : leftBone?.threshold,
      rightAirNoResponse: rightAir?.noResponse,
      leftAirNoResponse: leftAir?.noResponse,
    };
  });
};

// Custom dot for audiogram symbols
const RightEarDot = (props: any) => {
  const { cx, cy, payload } = props;
  if (payload?.rightAirNoResponse) {
    // Arrow down for no response
    return (
      <g>
        <circle cx={cx} cy={cy} r={6} fill="#EF4444" stroke="#EF4444" />
        <path d={`M${cx},${cy - 3} L${cx},${cy + 3} M${cx - 2},${cy + 1} L${cx},${cy + 3} L${cx + 2},${cy + 1}`} stroke="white" strokeWidth={1.5} fill="none" />
      </g>
    );
  }
  // Circle for right ear
  return <circle cx={cx} cy={cy} r={6} fill="#EF4444" stroke="#EF4444" strokeWidth={2} />;
};

const LeftEarDot = (props: any) => {
  const { cx, cy, payload } = props;
  if (payload?.leftAirNoResponse) {
    return (
      <g>
        <text x={cx} y={cy + 4} textAnchor="middle" fill="#3B82F6" fontSize={14} fontWeight="bold">X</text>
        <path d={`M${cx},${cy + 6} L${cx},${cy + 10}`} stroke="#3B82F6" strokeWidth={1.5} />
      </g>
    );
  }
  // X for left ear
  return (
    <text x={cx} y={cy + 5} textAnchor="middle" fill="#3B82F6" fontSize={16} fontWeight="bold">
      ×
    </text>
  );
};

// Bone conduction symbols
const RightBoneDot = (props: any) => {
  const { cx, cy } = props;
  // < symbol for right bone
  return (
    <text x={cx} y={cy + 4} textAnchor="middle" fill="#EF4444" fontSize={14}>
      {'<'}
    </text>
  );
};

const LeftBoneDot = (props: any) => {
  const { cx, cy } = props;
  // > symbol for left bone
  return (
    <text x={cx} y={cy + 4} textAnchor="middle" fill="#3B82F6" fontSize={14}>
      {'>'}
    </text>
  );
};

// Custom tooltip
const CustomTooltip = ({ active, payload, label }: any) => {
  if (!active || !payload?.length) return null;

  return (
    <div className="bg-white p-3 border border-gray-200 rounded-lg shadow-lg">
      <p className="font-semibold text-gray-900 mb-2">{label} Hz</p>
      {payload.map((entry: any, index: number) => (
        <p key={index} style={{ color: entry.color }} className="text-sm">
          {entry.name}: {entry.value !== null ? `${entry.value} dB` : 'No Response'}
        </p>
      ))}
    </div>
  );
};

export const AudiogramChart = ({
  audiogram,
  showBoneConduction = true,
  showNormalRange = true,
  height = 400,
}: AudiogramChartProps) => {
  const chartData = useMemo(() => transformData(audiogram), [audiogram]);

  return (
    <div className="bg-white rounded-lg p-4">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-lg font-semibold text-gray-900">Audiogram</h3>
        <div className="flex items-center space-x-4 text-sm">
          <div className="flex items-center">
            <span className="w-4 h-4 rounded-full bg-red-500 mr-2"></span>
            <span>Right Ear (O)</span>
          </div>
          <div className="flex items-center">
            <span className="text-blue-500 font-bold mr-2">×</span>
            <span>Left Ear (X)</span>
          </div>
        </div>
      </div>

      <ResponsiveContainer width="100%" height={height}>
        <LineChart
          data={chartData}
          margin={{ top: 20, right: 30, left: 20, bottom: 20 }}
        >
          <CartesianGrid strokeDasharray="3 3" stroke="#E5E7EB" />

          {/* X-axis: Frequency */}
          <XAxis
            dataKey="frequencyLabel"
            tick={{ fill: '#6B7280', fontSize: 12 }}
            axisLine={{ stroke: '#9CA3AF' }}
            tickLine={{ stroke: '#9CA3AF' }}
            label={{
              value: 'Frequency (Hz)',
              position: 'insideBottom',
              offset: -10,
              fill: '#6B7280',
            }}
          />

          {/* Y-axis: Hearing Level (inverted) */}
          <YAxis
            domain={[-10, 120]}
            reversed
            ticks={[-10, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120]}
            tick={{ fill: '#6B7280', fontSize: 12 }}
            axisLine={{ stroke: '#9CA3AF' }}
            tickLine={{ stroke: '#9CA3AF' }}
            label={{
              value: 'Hearing Level (dB HL)',
              angle: -90,
              position: 'insideLeft',
              fill: '#6B7280',
            }}
          />

          {/* Normal hearing range shading */}
          {showNormalRange && (
            <ReferenceLine y={25} stroke="#10B981" strokeDasharray="5 5" label={{ value: 'Normal limit', fill: '#10B981', fontSize: 10 }} />
          )}

          {/* Hearing loss grade lines */}
          <ReferenceLine y={40} stroke="#F59E0B" strokeDasharray="3 3" strokeOpacity={0.5} />
          <ReferenceLine y={55} stroke="#F97316" strokeDasharray="3 3" strokeOpacity={0.5} />
          <ReferenceLine y={70} stroke="#EF4444" strokeDasharray="3 3" strokeOpacity={0.5} />
          <ReferenceLine y={90} stroke="#DC2626" strokeDasharray="3 3" strokeOpacity={0.5} />

          <Tooltip content={<CustomTooltip />} />
          <Legend />

          {/* Right ear air conduction */}
          <Line
            type="linear"
            dataKey="rightAir"
            name="Right Air"
            stroke="#EF4444"
            strokeWidth={2}
            dot={<RightEarDot />}
            connectNulls={false}
          />

          {/* Left ear air conduction */}
          <Line
            type="linear"
            dataKey="leftAir"
            name="Left Air"
            stroke="#3B82F6"
            strokeWidth={2}
            dot={<LeftEarDot />}
            connectNulls={false}
          />

          {/* Bone conduction (if available and enabled) */}
          {showBoneConduction && audiogram.rightEarBone && (
            <Line
              type="linear"
              dataKey="rightBone"
              name="Right Bone"
              stroke="#EF4444"
              strokeWidth={1}
              strokeDasharray="5 5"
              dot={<RightBoneDot />}
              connectNulls={false}
            />
          )}

          {showBoneConduction && audiogram.leftEarBone && (
            <Line
              type="linear"
              dataKey="leftBone"
              name="Left Bone"
              stroke="#3B82F6"
              strokeWidth={1}
              strokeDasharray="5 5"
              dot={<LeftBoneDot />}
              connectNulls={false}
            />
          )}
        </LineChart>
      </ResponsiveContainer>

      {/* Summary section */}
      <div className="mt-4 grid grid-cols-2 gap-4 text-sm">
        <div className="bg-red-50 rounded-lg p-3">
          <h4 className="font-medium text-red-800 mb-2">Right Ear</h4>
          <p className="text-red-700">
            PTA: {audiogram.rightPTA !== undefined ? `${audiogram.rightPTA} dB` : 'N/A'}
          </p>
          <p className="text-red-700">
            Grade: {audiogram.rightHearingLossGrade || 'N/A'}
          </p>
          {audiogram.rightSRT !== undefined && (
            <p className="text-red-700">SRT: {audiogram.rightSRT} dB</p>
          )}
          {audiogram.rightWRS !== undefined && (
            <p className="text-red-700">WRS: {audiogram.rightWRS}%</p>
          )}
        </div>
        <div className="bg-blue-50 rounded-lg p-3">
          <h4 className="font-medium text-blue-800 mb-2">Left Ear</h4>
          <p className="text-blue-700">
            PTA: {audiogram.leftPTA !== undefined ? `${audiogram.leftPTA} dB` : 'N/A'}
          </p>
          <p className="text-blue-700">
            Grade: {audiogram.leftHearingLossGrade || 'N/A'}
          </p>
          {audiogram.leftSRT !== undefined && (
            <p className="text-blue-700">SRT: {audiogram.leftSRT} dB</p>
          )}
          {audiogram.leftWRS !== undefined && (
            <p className="text-blue-700">WRS: {audiogram.leftWRS}%</p>
          )}
        </div>
      </div>

      {/* Interpretation */}
      {audiogram.interpretation && (
        <div className="mt-4 p-3 bg-gray-50 rounded-lg">
          <h4 className="font-medium text-gray-800 mb-1">Interpretation</h4>
          <p className="text-gray-600 text-sm">{audiogram.interpretation}</p>
        </div>
      )}

      {/* Recommendations */}
      {audiogram.recommendations && (
        <div className="mt-2 p-3 bg-yellow-50 rounded-lg">
          <h4 className="font-medium text-yellow-800 mb-1">Recommendations</h4>
          <p className="text-yellow-700 text-sm">{audiogram.recommendations}</p>
        </div>
      )}
    </div>
  );
};

// Helper component for hearing loss grade legend
export const HearingLossGradeLegend = () => (
  <div className="flex flex-wrap gap-2 text-xs">
    {GRADE_BOUNDARIES.map((grade, index) => (
      <div key={grade.label} className="flex items-center">
        <span
          className="w-3 h-3 rounded mr-1"
          style={{ backgroundColor: grade.color }}
        ></span>
        <span>
          {grade.label} ({index === 0 ? '≤' : '>'}{index > 0 ? GRADE_BOUNDARIES[index - 1].threshold : -10}-{grade.threshold} dB)
        </span>
      </div>
    ))}
  </div>
);

// Calculate Pure Tone Average (PTA)
export const calculatePTA = (dataPoints: AudiogramDataPoint[]): number | undefined => {
  const frequencies = [500, 1000, 2000, 4000] as const;
  const relevantPoints = dataPoints.filter(
    (p) => frequencies.includes(p.frequency as any) && !p.noResponse
  );

  if (relevantPoints.length < 3) return undefined;

  const sum = relevantPoints.reduce((acc, p) => acc + p.threshold, 0);
  return Math.round(sum / relevantPoints.length);
};

// Determine hearing loss grade from PTA
export const getHearingLossGrade = (pta: number | undefined): HearingLossGrade | undefined => {
  if (pta === undefined) return undefined;

  if (pta <= 25) return HearingLossGrade.Normal;
  if (pta <= 40) return HearingLossGrade.Mild;
  if (pta <= 55) return HearingLossGrade.Moderate;
  if (pta <= 70) return HearingLossGrade.ModeratelySevere;
  if (pta <= 90) return HearingLossGrade.Severe;
  return HearingLossGrade.Profound;
};
