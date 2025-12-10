import { useState, useCallback, useRef, useEffect } from 'react';
import type {
  WorkflowDesignModel,
  DesignerNode,
  DesignerEdge,
  NodeTypeCatalog,
  NodeTypeDefinition,
  WorkflowValidationResult,
} from './types';

interface WorkflowDesignerProps {
  workflow: WorkflowDesignModel;
  nodeTypes: NodeTypeCatalog;
  onChange: (workflow: WorkflowDesignModel) => void;
  onValidate?: () => Promise<WorkflowValidationResult>;
  onSave?: () => void;
  readOnly?: boolean;
}

export function WorkflowDesigner({
  workflow,
  nodeTypes,
  onChange,
  onValidate,
  onSave,
  readOnly = false,
}: WorkflowDesignerProps) {
  const canvasRef = useRef<HTMLDivElement>(null);
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);
  const [selectedEdgeId, setSelectedEdgeId] = useState<string | null>(null);
  const [draggingNodeId, setDraggingNodeId] = useState<string | null>(null);
  const [connecting, setConnecting] = useState<{ nodeId: string; port: string } | null>(null);
  const [viewport, setViewport] = useState({ x: 0, y: 0, zoom: 1 });
  const [validationResult, setValidationResult] = useState<WorkflowValidationResult | null>(null);
  const [showProperties, setShowProperties] = useState(true);
  const [showPalette, setShowPalette] = useState(true);

  const selectedNode = selectedNodeId
    ? workflow.nodes.find((n) => n.id === selectedNodeId)
    : null;

  const selectedEdge = selectedEdgeId
    ? workflow.edges.find((e) => e.id === selectedEdgeId)
    : null;

  // Handle node drag
  const handleNodeMouseDown = useCallback(
    (e: React.MouseEvent, nodeId: string) => {
      if (readOnly) return;
      e.stopPropagation();
      setDraggingNodeId(nodeId);
      setSelectedNodeId(nodeId);
      setSelectedEdgeId(null);
    },
    [readOnly]
  );

  const handleMouseMove = useCallback(
    (e: React.MouseEvent) => {
      if (!draggingNodeId || readOnly) return;

      const node = workflow.nodes.find((n) => n.id === draggingNodeId);
      if (!node) return;

      const rect = canvasRef.current?.getBoundingClientRect();
      if (!rect) return;

      const x = (e.clientX - rect.left - viewport.x) / viewport.zoom;
      const y = (e.clientY - rect.top - viewport.y) / viewport.zoom;

      const updatedNodes = workflow.nodes.map((n) =>
        n.id === draggingNodeId ? { ...n, position: { x, y } } : n
      );

      onChange({ ...workflow, nodes: updatedNodes });
    },
    [draggingNodeId, workflow, onChange, viewport, readOnly]
  );

  const handleMouseUp = useCallback(() => {
    setDraggingNodeId(null);
    setConnecting(null);
  }, []);

  // Handle canvas click for deselection
  const handleCanvasClick = useCallback(() => {
    setSelectedNodeId(null);
    setSelectedEdgeId(null);
  }, []);

  // Handle edge selection
  const handleEdgeClick = useCallback((e: React.MouseEvent, edgeId: string) => {
    e.stopPropagation();
    setSelectedEdgeId(edgeId);
    setSelectedNodeId(null);
  }, []);

  // Add node from palette
  const handleAddNode = useCallback(
    (nodeType: NodeTypeDefinition) => {
      if (readOnly) return;

      const newNode: DesignerNode = {
        id: `${nodeType.type}_${Date.now()}`,
        type: nodeType.type,
        name: nodeType.name,
        description: '',
        position: { x: 250, y: 150 + workflow.nodes.length * 80 },
        dimensions: nodeType.defaultDimensions,
        style: nodeType.defaultStyle,
        config: {},
        inputMappings: {},
        outputMappings: {},
        isStart: nodeType.type === 'start',
        isEnd: nodeType.type === 'end',
        data: {},
      };

      onChange({ ...workflow, nodes: [...workflow.nodes, newNode] });
      setSelectedNodeId(newNode.id);
    },
    [workflow, onChange, readOnly]
  );

  // Delete selected node/edge
  const handleDelete = useCallback(() => {
    if (readOnly) return;

    if (selectedNodeId) {
      const updatedNodes = workflow.nodes.filter((n) => n.id !== selectedNodeId);
      const updatedEdges = workflow.edges.filter(
        (e) => e.source !== selectedNodeId && e.target !== selectedNodeId
      );
      onChange({ ...workflow, nodes: updatedNodes, edges: updatedEdges });
      setSelectedNodeId(null);
    } else if (selectedEdgeId) {
      const updatedEdges = workflow.edges.filter((e) => e.id !== selectedEdgeId);
      onChange({ ...workflow, edges: updatedEdges });
      setSelectedEdgeId(null);
    }
  }, [selectedNodeId, selectedEdgeId, workflow, onChange, readOnly]);

  // Handle keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Delete' || e.key === 'Backspace') {
        handleDelete();
      }
      if (e.key === 's' && (e.ctrlKey || e.metaKey)) {
        e.preventDefault();
        onSave?.();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleDelete, onSave]);

  // Validate workflow
  const handleValidate = async () => {
    if (onValidate) {
      const result = await onValidate();
      setValidationResult(result);
    }
  };

  // Update node property
  const updateNodeProperty = useCallback(
    (nodeId: string, property: string, value: unknown) => {
      if (readOnly) return;

      const updatedNodes = workflow.nodes.map((n) =>
        n.id === nodeId ? { ...n, [property]: value } : n
      );
      onChange({ ...workflow, nodes: updatedNodes });
    },
    [workflow, onChange, readOnly]
  );

  // Update node config
  const updateNodeConfig = useCallback(
    (nodeId: string, configKey: string, value: unknown) => {
      if (readOnly) return;

      const updatedNodes = workflow.nodes.map((n) =>
        n.id === nodeId ? { ...n, config: { ...n.config, [configKey]: value } } : n
      );
      onChange({ ...workflow, nodes: updatedNodes });
    },
    [workflow, onChange, readOnly]
  );

  // Create connection between nodes
  const handleCreateConnection = useCallback(
    (sourceId: string, targetId: string) => {
      if (readOnly) return;
      if (sourceId === targetId) return;

      // Check if connection already exists
      const exists = workflow.edges.some(
        (e) => e.source === sourceId && e.target === targetId
      );
      if (exists) return;

      const newEdge: DesignerEdge = {
        id: `edge_${Date.now()}`,
        source: sourceId,
        target: targetId,
        type: 'default',
        isDefault: false,
        priority: 0,
        animated: false,
      };

      onChange({ ...workflow, edges: [...workflow.edges, newEdge] });
    },
    [workflow, onChange, readOnly]
  );

  // Render node
  const renderNode = (node: DesignerNode) => {
    const isSelected = node.id === selectedNodeId;
    const nodeTypeDef = nodeTypes.categories
      .flatMap((c) => c.nodeTypes)
      .find((t) => t.type === node.type);

    const width = node.dimensions?.width ?? 180;
    const height = node.dimensions?.height ?? 40;
    const bgColor = node.style?.backgroundColor ?? '#2196F3';
    const borderColor = node.style?.borderColor ?? '#1565C0';
    const textColor = node.style?.textColor ?? '#FFFFFF';
    const borderRadius = node.style?.borderRadius ?? 4;

    // Special rendering for gateways (diamond shape)
    const isGateway = node.type.toLowerCase().includes('gateway');

    const hasError = validationResult?.errors.some((e) => e.nodeId === node.id);
    const hasWarning = validationResult?.warnings.some((w) => w.nodeId === node.id);

    return (
      <g
        key={node.id}
        transform={`translate(${node.position.x}, ${node.position.y})`}
        onMouseDown={(e) => handleNodeMouseDown(e, node.id)}
        onClick={(e) => {
          e.stopPropagation();
          setSelectedNodeId(node.id);
          setSelectedEdgeId(null);
        }}
        style={{ cursor: readOnly ? 'default' : 'move' }}
      >
        {/* Node shape */}
        {isGateway ? (
          <polygon
            points={`${width / 2},0 ${width},${height / 2} ${width / 2},${height} 0,${height / 2}`}
            fill={bgColor}
            stroke={isSelected ? '#FF5722' : hasError ? '#F44336' : hasWarning ? '#FF9800' : borderColor}
            strokeWidth={isSelected ? 3 : 2}
          />
        ) : node.type === 'start' || node.type === 'end' ? (
          <circle
            cx={width / 2}
            cy={height / 2}
            r={Math.min(width, height) / 2}
            fill={bgColor}
            stroke={isSelected ? '#FF5722' : hasError ? '#F44336' : hasWarning ? '#FF9800' : borderColor}
            strokeWidth={isSelected ? 3 : 2}
          />
        ) : (
          <rect
            width={width}
            height={height}
            rx={borderRadius}
            ry={borderRadius}
            fill={bgColor}
            stroke={isSelected ? '#FF5722' : hasError ? '#F44336' : hasWarning ? '#FF9800' : borderColor}
            strokeWidth={isSelected ? 3 : 2}
          />
        )}

        {/* Node label */}
        <text
          x={width / 2}
          y={height / 2}
          textAnchor="middle"
          dominantBaseline="middle"
          fill={textColor}
          fontSize={12}
          fontWeight={500}
          style={{ pointerEvents: 'none' }}
        >
          {node.name.length > 20 ? `${node.name.substring(0, 18)}...` : node.name}
        </text>

        {/* Connection handles */}
        {!readOnly && (
          <>
            {/* Input handle (top) */}
            {node.type !== 'start' && (
              <circle
                cx={width / 2}
                cy={-5}
                r={6}
                fill="#fff"
                stroke="#666"
                strokeWidth={1}
                style={{ cursor: 'crosshair' }}
                onClick={(e) => {
                  e.stopPropagation();
                  if (connecting) {
                    handleCreateConnection(connecting.nodeId, node.id);
                    setConnecting(null);
                  }
                }}
              />
            )}

            {/* Output handle (bottom) */}
            {node.type !== 'end' && (
              <circle
                cx={width / 2}
                cy={height + 5}
                r={6}
                fill="#fff"
                stroke="#666"
                strokeWidth={1}
                style={{ cursor: 'crosshair' }}
                onClick={(e) => {
                  e.stopPropagation();
                  setConnecting({ nodeId: node.id, port: 'out' });
                }}
              />
            )}
          </>
        )}

        {/* Type icon */}
        {nodeTypeDef?.iconClass && (
          <text
            x={10}
            y={height / 2}
            dominantBaseline="middle"
            fill={textColor}
            fontSize={14}
            style={{ pointerEvents: 'none' }}
          >
            {getIconForType(nodeTypeDef.iconClass)}
          </text>
        )}
      </g>
    );
  };

  // Render edge
  const renderEdge = (edge: DesignerEdge) => {
    const sourceNode = workflow.nodes.find((n) => n.id === edge.source);
    const targetNode = workflow.nodes.find((n) => n.id === edge.target);

    if (!sourceNode || !targetNode) return null;

    const sourceWidth = sourceNode.dimensions?.width ?? 180;
    const sourceHeight = sourceNode.dimensions?.height ?? 40;
    const targetWidth = targetNode.dimensions?.width ?? 180;

    const startX = sourceNode.position.x + sourceWidth / 2;
    const startY = sourceNode.position.y + sourceHeight + 5;
    const endX = targetNode.position.x + targetWidth / 2;
    const endY = targetNode.position.y - 5;

    const isSelected = edge.id === selectedEdgeId;
    const midY = (startY + endY) / 2;

    // Create a curved path
    const path = `M ${startX} ${startY} C ${startX} ${midY}, ${endX} ${midY}, ${endX} ${endY}`;

    return (
      <g key={edge.id} onClick={(e) => handleEdgeClick(e, edge.id)}>
        {/* Invisible wider path for easier clicking */}
        <path
          d={path}
          fill="none"
          stroke="transparent"
          strokeWidth={15}
          style={{ cursor: 'pointer' }}
        />
        {/* Visible path */}
        <path
          d={path}
          fill="none"
          stroke={isSelected ? '#FF5722' : edge.style?.strokeColor ?? '#666'}
          strokeWidth={isSelected ? 3 : edge.style?.strokeWidth ?? 2}
          strokeDasharray={edge.style?.strokeDasharray}
          markerEnd="url(#arrowhead)"
        />
        {/* Edge label */}
        {edge.label && (
          <text
            x={(startX + endX) / 2}
            y={midY - 10}
            textAnchor="middle"
            fontSize={10}
            fill="#666"
            style={{
              backgroundColor: edge.style?.labelBackgroundColor ?? '#fff',
              pointerEvents: 'none',
            }}
          >
            {edge.label}
          </text>
        )}
        {/* Condition indicator */}
        {edge.condition && (
          <text
            x={(startX + endX) / 2}
            y={midY + 10}
            textAnchor="middle"
            fontSize={9}
            fill="#999"
            fontStyle="italic"
          >
            [{edge.condition.length > 15 ? `${edge.condition.substring(0, 13)}...` : edge.condition}]
          </text>
        )}
      </g>
    );
  };

  return (
    <div className="flex h-full bg-gray-100">
      {/* Left Palette */}
      {showPalette && (
        <div className="w-64 bg-white border-r border-gray-200 overflow-y-auto">
          <div className="p-4 border-b border-gray-200">
            <h3 className="font-semibold text-gray-900">Components</h3>
          </div>
          {nodeTypes.categories.map((category) => (
            <div key={category.id} className="border-b border-gray-100">
              <div className="px-4 py-2 bg-gray-50 text-sm font-medium text-gray-700">
                {category.name}
              </div>
              <div className="p-2 space-y-1">
                {category.nodeTypes.map((nodeType) => (
                  <button
                    key={nodeType.type}
                    onClick={() => handleAddNode(nodeType)}
                    disabled={readOnly}
                    className="w-full px-3 py-2 text-left text-sm rounded-md hover:bg-gray-100 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                    style={{
                      borderLeft: `3px solid ${nodeType.defaultStyle?.backgroundColor ?? '#2196F3'}`,
                    }}
                  >
                    <span>{getIconForType(nodeType.iconClass)}</span>
                    <span>{nodeType.name}</span>
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Main Canvas */}
      <div className="flex-1 flex flex-col">
        {/* Toolbar */}
        <div className="bg-white border-b border-gray-200 px-4 py-2 flex items-center justify-between">
          <div className="flex items-center gap-2">
            <button
              onClick={() => setShowPalette(!showPalette)}
              className="px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100 rounded"
            >
              {showPalette ? 'Hide' : 'Show'} Palette
            </button>
            <button
              onClick={() => setShowProperties(!showProperties)}
              className="px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100 rounded"
            >
              {showProperties ? 'Hide' : 'Show'} Properties
            </button>
            <span className="text-gray-300">|</span>
            <button
              onClick={() => setViewport((v) => ({ ...v, zoom: Math.min(v.zoom + 0.1, 2) }))}
              className="px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100 rounded"
            >
              Zoom In
            </button>
            <button
              onClick={() => setViewport((v) => ({ ...v, zoom: Math.max(v.zoom - 0.1, 0.5) }))}
              className="px-3 py-1.5 text-sm text-gray-600 hover:bg-gray-100 rounded"
            >
              Zoom Out
            </button>
            <span className="text-sm text-gray-500">{Math.round(viewport.zoom * 100)}%</span>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={handleValidate}
              className="px-4 py-1.5 text-sm bg-blue-50 text-blue-600 hover:bg-blue-100 rounded"
            >
              Validate
            </button>
            {!readOnly && (
              <button
                onClick={onSave}
                className="px-4 py-1.5 text-sm bg-blue-600 text-white hover:bg-blue-700 rounded"
              >
                Save
              </button>
            )}
          </div>
        </div>

        {/* Validation Messages */}
        {validationResult && !validationResult.isValid && (
          <div className="bg-red-50 border-b border-red-200 px-4 py-2">
            <div className="text-sm text-red-600">
              {validationResult.errors.length} error(s), {validationResult.warnings.length} warning(s)
            </div>
          </div>
        )}

        {/* Canvas */}
        <div
          ref={canvasRef}
          className="flex-1 overflow-hidden"
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseUp}
          onClick={handleCanvasClick}
          style={{ background: 'linear-gradient(#e5e5e5 1px, transparent 1px), linear-gradient(90deg, #e5e5e5 1px, transparent 1px)', backgroundSize: '20px 20px' }}
        >
          <svg
            width="100%"
            height="100%"
            style={{
              transform: `translate(${viewport.x}px, ${viewport.y}px) scale(${viewport.zoom})`,
              transformOrigin: '0 0',
            }}
          >
            {/* Arrow marker definition */}
            <defs>
              <marker
                id="arrowhead"
                markerWidth="10"
                markerHeight="7"
                refX="9"
                refY="3.5"
                orient="auto"
              >
                <polygon points="0 0, 10 3.5, 0 7" fill="#666" />
              </marker>
            </defs>

            {/* Render edges first (below nodes) */}
            {workflow.edges.map(renderEdge)}

            {/* Render nodes */}
            {workflow.nodes.map(renderNode)}

            {/* Connection line when dragging */}
            {connecting && (
              <line
                x1={
                  (workflow.nodes.find((n) => n.id === connecting.nodeId)?.position.x ?? 0) +
                  ((workflow.nodes.find((n) => n.id === connecting.nodeId)?.dimensions?.width ?? 180) / 2)
                }
                y1={
                  (workflow.nodes.find((n) => n.id === connecting.nodeId)?.position.y ?? 0) +
                  (workflow.nodes.find((n) => n.id === connecting.nodeId)?.dimensions?.height ?? 40) +
                  5
                }
                x2={viewport.x}
                y2={viewport.y}
                stroke="#666"
                strokeDasharray="5,5"
              />
            )}
          </svg>
        </div>
      </div>

      {/* Right Properties Panel */}
      {showProperties && (
        <div className="w-80 bg-white border-l border-gray-200 overflow-y-auto">
          <div className="p-4 border-b border-gray-200">
            <h3 className="font-semibold text-gray-900">Properties</h3>
          </div>

          {selectedNode ? (
            <div className="p-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
                <input
                  type="text"
                  value={selectedNode.name}
                  onChange={(e) => updateNodeProperty(selectedNode.id, 'name', e.target.value)}
                  disabled={readOnly}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
                <textarea
                  value={selectedNode.description ?? ''}
                  onChange={(e) => updateNodeProperty(selectedNode.id, 'description', e.target.value)}
                  disabled={readOnly}
                  rows={2}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                <input
                  type="text"
                  value={selectedNode.type}
                  disabled
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm bg-gray-50"
                />
              </div>

              {/* Type-specific properties */}
              {renderNodeTypeProperties(selectedNode, updateNodeConfig, readOnly, nodeTypes)}

              {/* Delete button */}
              {!readOnly && (
                <button
                  onClick={handleDelete}
                  className="w-full px-4 py-2 text-sm text-red-600 border border-red-300 rounded-md hover:bg-red-50"
                >
                  Delete Node
                </button>
              )}
            </div>
          ) : selectedEdge ? (
            <div className="p-4 space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Label</label>
                <input
                  type="text"
                  value={selectedEdge.label ?? ''}
                  onChange={(e) => {
                    const updatedEdges = workflow.edges.map((edge) =>
                      edge.id === selectedEdge.id ? { ...edge, label: e.target.value } : edge
                    );
                    onChange({ ...workflow, edges: updatedEdges });
                  }}
                  disabled={readOnly}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Condition</label>
                <textarea
                  value={selectedEdge.condition ?? ''}
                  onChange={(e) => {
                    const updatedEdges = workflow.edges.map((edge) =>
                      edge.id === selectedEdge.id ? { ...edge, condition: e.target.value } : edge
                    );
                    onChange({ ...workflow, edges: updatedEdges });
                  }}
                  disabled={readOnly}
                  rows={2}
                  placeholder="e.g., var.status == 'approved'"
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                />
              </div>

              <div className="flex items-center gap-2">
                <input
                  type="checkbox"
                  id="isDefault"
                  checked={selectedEdge.isDefault}
                  onChange={(e) => {
                    const updatedEdges = workflow.edges.map((edge) =>
                      edge.id === selectedEdge.id ? { ...edge, isDefault: e.target.checked } : edge
                    );
                    onChange({ ...workflow, edges: updatedEdges });
                  }}
                  disabled={readOnly}
                  className="h-4 w-4 rounded border-gray-300"
                />
                <label htmlFor="isDefault" className="text-sm text-gray-700">
                  Default path
                </label>
              </div>

              {!readOnly && (
                <button
                  onClick={handleDelete}
                  className="w-full px-4 py-2 text-sm text-red-600 border border-red-300 rounded-md hover:bg-red-50"
                >
                  Delete Connection
                </button>
              )}
            </div>
          ) : (
            <div className="p-4 text-sm text-gray-500">
              Select a node or connection to view its properties
            </div>
          )}
        </div>
      )}
    </div>
  );
}

// Helper function to render type-specific properties
function renderNodeTypeProperties(
  node: DesignerNode,
  updateConfig: (nodeId: string, key: string, value: unknown) => void,
  readOnly: boolean,
  nodeTypes: NodeTypeCatalog
) {
  const nodeTypeDef = nodeTypes.categories
    .flatMap((c) => c.nodeTypes)
    .find((t) => t.type === node.type);

  if (!nodeTypeDef?.properties.length) return null;

  return (
    <div className="border-t border-gray-200 pt-4 mt-4">
      <h4 className="text-sm font-medium text-gray-700 mb-3">Configuration</h4>
      <div className="space-y-3">
        {nodeTypeDef.properties
          .filter((p) => !p.isAdvanced)
          .map((prop) => (
            <div key={prop.name}>
              <label className="block text-sm font-medium text-gray-600 mb-1">
                {prop.displayName}
                {prop.isRequired && <span className="text-red-500">*</span>}
              </label>
              {prop.type === 'select' && prop.options ? (
                <select
                  value={(node.config[prop.name] as string) ?? prop.defaultValue ?? ''}
                  onChange={(e) => updateConfig(node.id, prop.name, e.target.value)}
                  disabled={readOnly}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                >
                  <option value="">Select...</option>
                  {prop.options.map((opt) => (
                    <option key={opt.value} value={opt.value}>
                      {opt.label}
                    </option>
                  ))}
                </select>
              ) : prop.type === 'boolean' ? (
                <input
                  type="checkbox"
                  checked={(node.config[prop.name] as boolean) ?? prop.defaultValue ?? false}
                  onChange={(e) => updateConfig(node.id, prop.name, e.target.checked)}
                  disabled={readOnly}
                  className="h-4 w-4 rounded border-gray-300"
                />
              ) : prop.type === 'number' ? (
                <input
                  type="number"
                  value={(node.config[prop.name] as number) ?? prop.defaultValue ?? ''}
                  onChange={(e) => updateConfig(node.id, prop.name, parseFloat(e.target.value))}
                  disabled={readOnly}
                  placeholder={prop.placeholder}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                />
              ) : prop.type === 'code' || prop.type === 'json' ? (
                <textarea
                  value={(node.config[prop.name] as string) ?? ''}
                  onChange={(e) => updateConfig(node.id, prop.name, e.target.value)}
                  disabled={readOnly}
                  rows={4}
                  placeholder={prop.placeholder}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm font-mono disabled:bg-gray-50"
                />
              ) : (
                <input
                  type="text"
                  value={(node.config[prop.name] as string) ?? ''}
                  onChange={(e) => updateConfig(node.id, prop.name, e.target.value)}
                  disabled={readOnly}
                  placeholder={prop.placeholder}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md text-sm disabled:bg-gray-50"
                />
              )}
              {prop.description && (
                <p className="mt-1 text-xs text-gray-500">{prop.description}</p>
              )}
            </div>
          ))}
      </div>
    </div>
  );
}

// Helper function to get icon character for icon class
function getIconForType(iconClass?: string): string {
  if (!iconClass) return '';

  const iconMap: Record<string, string> = {
    'fa-play-circle': '▶',
    'fa-stop-circle': '⏹',
    'fa-square': '▢',
    'fa-cogs': '⚙',
    'fa-user': '👤',
    'fa-code': '{ }',
    'fa-times': '✕',
    'fa-plus': '+',
    'fa-circle': '○',
    'fa-clock': '⏰',
    'fa-satellite-dish': '📡',
    'fa-broadcast-tower': '📻',
    'fa-external-link-alt': '↗',
    'fa-layer-group': '☰',
    'fa-equals': '=',
    'fa-bell': '🔔',
    'fa-globe': '🌐',
    'fa-envelope': '✉',
  };

  return iconMap[iconClass] ?? '•';
}

export default WorkflowDesigner;
