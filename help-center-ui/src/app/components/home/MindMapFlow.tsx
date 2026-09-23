"use client";

import React, { useState, useMemo, useRef, useEffect, useCallback } from "react";
import { AnimatePresence, motion } from "framer-motion";
import {
  ZoomIn,
  ZoomOut,
  RotateCcw,
  GitFork,
  ListOrdered,
  FileText,
  ChevronRight,
} from "lucide-react";
import { cn } from "@/lib/utils";
import type { PublicGuide as Guide } from "@/services/public/PublicGuideService";
import type { PublicModule as Module } from "@/services/public/PublicModuleService";

interface MindMapFlowProps {
  projectName?: string;
  modules?: Module[];
  guides: Guide[];
  selectedModule?: string;
  onSelectGuide: (guide: Guide) => void;
}

// Geometric constants for NotebookLM style item-to-item layout
const GUIDE_WIDTH = 260;
const NODE_HEIGHT = 52;
const VERTICAL_GAP = 20;
const ROW_HEIGHT = NODE_HEIGHT + VERTICAL_GAP;
const COLUMN_GAP = 90;

// Mirrors the tree container's Tailwind heights below (h-[520px] sm:h-[620px]).
// The value is fixed by CSS rather than measured from the DOM, so the initial
// pan/zoom can be derived from the viewport width alone.
const DESKTOP_CONTAINER_HEIGHT = 620;

interface MindMapView {
  pan: { x: number; y: number };
  zoom: number;
}

// Pure function of the viewport — safe to call during render (initial state,
// or when reacting to a prop change) without touching the DOM or refs.
function getInitialMindMapView(): MindMapView {
  const isMobile = typeof window !== "undefined" && window.innerWidth < 768;
  if (isMobile) {
    return { pan: { x: 16, y: 30 }, zoom: 0.65 };
  }
  return {
    pan: { x: 60, y: Math.max(40, DESKTOP_CONTAINER_HEIGHT / 2 - 120) },
    zoom: 1,
  };
}

interface TreeNode {
  guide: Guide;
  depth: number;
  stepNumber: number;
  children: TreeNode[];
}

interface PlacedNode {
  guide: Guide;
  depth: number;
  stepNumber: number;
  x: number;
  y: number;
  width: number;
  height: number;
  hasChildren: boolean;
  isExpanded: boolean;
}

interface Connection {
  id: string;
  d: string;
}

export const MindMapFlow: React.FC<MindMapFlowProps> = ({
  guides = [],
  selectedModule = "",
  onSelectGuide,
}) => {
  const containerRef = useRef<HTMLDivElement>(null);

  // View mode: 'tree' for NotebookLM item-to-item tree, 'list' for mobile list flow
  const [viewMode, setViewMode] = useState<"tree" | "list">("tree");

  // Filter guides if module selected
  const activeGuides = useMemo(() => {
    if (!selectedModule) return guides;
    return guides.filter((g) => g.module === selectedModule);
  }, [guides, selectedModule]);

  // Build item-to-item tree from previousGuidePublicId links
  const treeRoots = useMemo(() => {
    const guideMap = new Map<string, Guide>();
    activeGuides.forEach((g) => guideMap.set(g.publicId, g));

    const childrenMap = new Map<string, Guide[]>();
    const roots: Guide[] = [];

    // Separate into root items and children items
    activeGuides.forEach((g) => {
      if (g.previousGuidePublicId && guideMap.has(g.previousGuidePublicId)) {
        const parentId = g.previousGuidePublicId;
        if (!childrenMap.has(parentId)) childrenMap.set(parentId, []);
        childrenMap.get(parentId)!.push(g);
      } else {
        roots.push(g);
      }
    });

    // Sort roots by title or default sequence
    roots.sort((a, b) => a.title.localeCompare(b.title, "tr"));

    let globalStep = 1;
    const visited = new Set<string>();

    function buildNode(guide: Guide, depth: number): TreeNode {
      visited.add(guide.publicId);
      const stepNumber = globalStep++;
      const rawChildren = childrenMap.get(guide.publicId) || [];
      const children: TreeNode[] = [];

      rawChildren.forEach((childGuide) => {
        if (!visited.has(childGuide.publicId)) {
          children.push(buildNode(childGuide, depth + 1));
        }
      });

      return {
        guide,
        depth,
        stepNumber,
        children,
      };
    }

    return roots.map((root) => buildNode(root, 0));
  }, [activeGuides]);

  // Expanded states for item nodes
  const [expandedNodes, setExpandedNodes] = useState<Record<string, boolean>>({});

  const toggleNode = (guidePublicId: string) => {
    setExpandedNodes((prev) => ({
      ...prev,
      [guidePublicId]: prev[guidePublicId] === false ? true : false,
    }));
  };

  const handleExpandAll = () => {
    const updated: Record<string, boolean> = {};
    activeGuides.forEach((g) => {
      updated[g.publicId] = true;
    });
    setExpandedNodes(updated);
  };

  const handleCollapseAll = () => {
    const updated: Record<string, boolean> = {};
    activeGuides.forEach((g) => {
      updated[g.publicId] = false;
    });
    setExpandedNodes(updated);
  };

  // Pan and Zoom — initialised responsively from the viewport up front.
  const [pan, setPan] = useState(() => getInitialMindMapView().pan);
  const [zoom, setZoom] = useState(() => getInitialMindMapView().zoom);
  const [isDragging, setIsDragging] = useState(false);
  const [dragStart, setDragStart] = useState({ x: 0, y: 0 });

  // Mirrors of pan/zoom for the native (non-React) event listeners below, which
  // must read the latest value without re-subscribing on every pan/zoom change.
  // Kept in sync via an effect — never written during render.
  const panRef = useRef(pan);
  const zoomRef = useRef(zoom);
  useEffect(() => {
    panRef.current = pan;
    zoomRef.current = zoom;
  }, [pan, zoom]);

  // Reset view to center with responsive initial zoom (wired to the toolbar button)
  const handleResetView = useCallback(() => {
    const view = getInitialMindMapView();
    setZoom(view.zoom);
    setPan(view.pan);
  }, []);

  // Reset the view whenever the active module filter changes. Adjusted directly
  // during render (comparing against the previous prop value) instead of in an
  // effect, since this is state derived from a prop change rather than a sync
  // with an external system. See: https://react.dev/reference/react/useState#storing-information-from-previous-renders
  const [prevSelectedModuleForView, setPrevSelectedModuleForView] = useState(selectedModule);
  if (selectedModule !== prevSelectedModuleForView) {
    setPrevSelectedModuleForView(selectedModule);
    const view = getInitialMindMapView();
    setZoom(view.zoom);
    setPan(view.pan);
  }

  // Tree Layout Calculation (Item to Item direct connections)
  const layout = useMemo(() => {
    let currentY = 40;
    const placedNodes: PlacedNode[] = [];
    const connections: Connection[] = [];

    function layoutSubtree(node: TreeNode): { y: number; height: number } {
      const isExp = expandedNodes[node.guide.publicId] !== false;
      const hasKids = node.children.length > 0;
      const nodeX = 40 + node.depth * (GUIDE_WIDTH + COLUMN_GAP);

      if (!hasKids || !isExp) {
        const nodeY = currentY;
        currentY += ROW_HEIGHT;
        placedNodes.push({
          guide: node.guide,
          depth: node.depth,
          stepNumber: node.stepNumber,
          x: nodeX,
          y: nodeY,
          width: GUIDE_WIDTH,
          height: NODE_HEIGHT,
          hasChildren: hasKids,
          isExpanded: isExp,
        });
        return { y: nodeY, height: ROW_HEIGHT };
      }

      // If has visible expanded children: layout children first
      const childYs: number[] = [];
      node.children.forEach((child) => {
        const childLayout = layoutSubtree(child);
        childYs.push(childLayout.y);
      });

      const nodeY = (childYs[0] + childYs[childYs.length - 1]) / 2;
      placedNodes.push({
        guide: node.guide,
        depth: node.depth,
        stepNumber: node.stepNumber,
        x: nodeX,
        y: nodeY,
        width: GUIDE_WIDTH,
        height: NODE_HEIGHT,
        hasChildren: true,
        isExpanded: true,
      });

      // Connections from this guide item directly to its child guide items
      const startX = nodeX + GUIDE_WIDTH;
      const startY = nodeY + NODE_HEIGHT / 2;

      node.children.forEach((child) => {
        const childPlaced = placedNodes.find(
          (p) => p.guide.publicId === child.guide.publicId
        );
        if (childPlaced) {
          const endX = childPlaced.x;
          const endY = childPlaced.y + NODE_HEIGHT / 2;
          const midX = (startX + endX) / 2;
          const d = `M ${startX} ${startY} C ${midX} ${startY}, ${midX} ${endY}, ${endX} ${endY}`;
          connections.push({
            id: `${node.guide.publicId}->${child.guide.publicId}`,
            d,
          });
        }
      });

      return { y: nodeY, height: currentY - childYs[0] };
    }

    treeRoots.forEach((root) => {
      layoutSubtree(root);
    });

    const maxDepth = placedNodes.reduce((max, n) => Math.max(max, n.depth), 0);
    const totalWidth = 40 + (maxDepth + 1) * (GUIDE_WIDTH + COLUMN_GAP) + 120;
    const totalHeight = Math.max(currentY + 60, 560);

    return { placedNodes, connections, totalWidth, totalHeight };
  }, [treeRoots, expandedNodes]);

  // Desktop Mouse Pan
  const handleMouseDown = (e: React.MouseEvent) => {
    if (
      (e.target as HTMLElement).closest("button") ||
      (e.target as HTMLElement).closest("[data-clickable='true']")
    ) {
      return;
    }
    setIsDragging(true);
    setDragStart({ x: e.clientX - panRef.current.x, y: e.clientY - panRef.current.y });
  };

  const handleMouseMove = (e: React.MouseEvent) => {
    if (!isDragging) return;
    setPan({
      x: e.clientX - dragStart.x,
      y: e.clientY - dragStart.y,
    });
  };

  const handleMouseUp = () => {
    setIsDragging(false);
  };

  // Attach non-passive Ctrl + Wheel and Pinch-to-zoom listeners
  useEffect(() => {
    if (viewMode !== "tree") return;
    const el = containerRef.current;
    if (!el) return;

    // 1. Desktop: Ctrl + Mouse Wheel Zoom (Focal-point scaling)
    const handleNativeWheel = (e: WheelEvent) => {
      if (e.ctrlKey || e.metaKey) {
        e.preventDefault();
        const rect = el.getBoundingClientRect();
        const focalX = e.clientX - rect.left;
        const focalY = e.clientY - rect.top;

        const delta = -e.deltaY * 0.0025;
        const prevZoom = zoomRef.current;
        const nextZoom = Math.min(Math.max(prevZoom + delta, 0.35), 2.2);

        if (nextZoom !== prevZoom) {
          const prevPan = panRef.current;
          const nextPanX = focalX - (focalX - prevPan.x) * (nextZoom / prevZoom);
          const nextPanY = focalY - (focalY - prevPan.y) * (nextZoom / prevZoom);

          setZoom(nextZoom);
          setPan({ x: nextPanX, y: nextPanY });
        }
      }
    };

    // 2. Mobile: Touch Pan & Pinch-to-Zoom (pinch in and out)
    let initialPinchDist: number | null = null;
    let initialPinchZoom = 1;
    let initialPinchPan = { x: 0, y: 0 };
    let initialFocal = { x: 0, y: 0 };
    let singleTouchStart: { x: number; y: number } | null = null;

    const handleNativeTouchStart = (e: TouchEvent) => {
      if (e.touches.length === 1) {
        const touch = e.touches[0];
        const target = touch.target as HTMLElement;
        if (target.closest("button") || target.closest("[data-clickable='true']")) {
          singleTouchStart = null;
          return;
        }
        setIsDragging(true);
        singleTouchStart = {
          x: touch.clientX - panRef.current.x,
          y: touch.clientY - panRef.current.y,
        };
      } else if (e.touches.length === 2) {
        e.preventDefault();
        singleTouchStart = null;
        setIsDragging(false);

        const t1 = e.touches[0];
        const t2 = e.touches[1];
        initialPinchDist = Math.hypot(t1.clientX - t2.clientX, t1.clientY - t2.clientY);
        initialPinchZoom = zoomRef.current;
        initialPinchPan = { ...panRef.current };

        const rect = el.getBoundingClientRect();
        initialFocal = {
          x: (t1.clientX + t2.clientX) / 2 - rect.left,
          y: (t1.clientY + t2.clientY) / 2 - rect.top,
        };
      }
    };

    const handleNativeTouchMove = (e: TouchEvent) => {
      if (e.touches.length === 1 && singleTouchStart) {
        const touch = e.touches[0];
        setPan({
          x: touch.clientX - singleTouchStart.x,
          y: touch.clientY - singleTouchStart.y,
        });
      } else if (e.touches.length === 2 && initialPinchDist && initialPinchDist > 0) {
        e.preventDefault();
        const t1 = e.touches[0];
        const t2 = e.touches[1];
        const currentDist = Math.hypot(t1.clientX - t2.clientX, t1.clientY - t2.clientY);
        const scale = currentDist / initialPinchDist;
        const nextZoom = Math.min(Math.max(initialPinchZoom * scale, 0.35), 2.2);

        const focalX = initialFocal.x;
        const focalY = initialFocal.y;
        const prevPan = initialPinchPan;
        const prevZoom = initialPinchZoom;

        const nextPanX = focalX - (focalX - prevPan.x) * (nextZoom / prevZoom);
        const nextPanY = focalY - (focalY - prevPan.y) * (nextZoom / prevZoom);

        setZoom(nextZoom);
        setPan({ x: nextPanX, y: nextPanY });
      }
    };

    const handleNativeTouchEnd = () => {
      singleTouchStart = null;
      initialPinchDist = null;
      setIsDragging(false);
    };

    el.addEventListener("wheel", handleNativeWheel, { passive: false });
    el.addEventListener("touchstart", handleNativeTouchStart, { passive: false });
    el.addEventListener("touchmove", handleNativeTouchMove, { passive: false });
    el.addEventListener("touchend", handleNativeTouchEnd);
    el.addEventListener("touchcancel", handleNativeTouchEnd);

    return () => {
      el.removeEventListener("wheel", handleNativeWheel);
      el.removeEventListener("touchstart", handleNativeTouchStart);
      el.removeEventListener("touchmove", handleNativeTouchMove);
      el.removeEventListener("touchend", handleNativeTouchEnd);
      el.removeEventListener("touchcancel", handleNativeTouchEnd);
    };
  }, [viewMode]);

  return (
    <div className="relative w-full flex flex-col rounded-2xl overflow-hidden bg-obsidian-950/80 border border-slate-800/80 shadow-2xl">
      {/* Top Controls Toolbar */}
      <div className="flex flex-wrap items-center justify-between gap-3 px-3.5 py-3 bg-obsidian-900/90 border-b border-slate-800/80 backdrop-blur-md z-20">
        <div className="flex items-center space-x-2.5">
          <div className="w-2.5 h-2.5 rounded-full bg-purple-400 shadow-glow-purple" />
          <span className="text-xs font-bold text-white tracking-wide">
            Zihin Haritası — Adım Bağlantıları
          </span>
          <span className="hidden sm:inline-block text-[11px] text-slate-400 font-medium">
            ({activeGuides.length} Kılavuz Bağlantısı)
          </span>
        </div>

        {/* View Mode Switcher (Tree vs Mobile List) */}
        <div className="flex items-center space-x-2">
          <div className="flex items-center bg-obsidian-850 border border-slate-700/60 rounded-xl p-0.5 shadow-sm">
            <button
              type="button"
              onClick={() => setViewMode("tree")}
              className={cn(
                "px-2.5 py-1 rounded-lg text-xs font-semibold transition flex items-center gap-1.5 cursor-pointer",
                viewMode === "tree"
                  ? "bg-purple-500/20 text-purple-300 border border-purple-500/40 shadow-sm font-bold"
                  : "text-slate-400 hover:text-white"
              )}
            >
              <GitFork size={12} />
              <span>Ağaç Bağlantıları</span>
            </button>
            <button
              type="button"
              onClick={() => setViewMode("list")}
              className={cn(
                "px-2.5 py-1 rounded-lg text-xs font-semibold transition flex items-center gap-1.5 cursor-pointer",
                viewMode === "list"
                  ? "bg-purple-500/20 text-purple-300 border border-purple-500/40 shadow-sm font-bold"
                  : "text-slate-400 hover:text-white"
              )}
            >
              <ListOrdered size={12} />
              <span>Sıralı Akış</span>
            </button>
          </div>

          {/* Tree-only controls */}
          {viewMode === "tree" && (
            <div className="hidden sm:flex items-center space-x-1.5">
              <button
                type="button"
                onClick={handleExpandAll}
                className="px-2 py-1 text-[11px] font-semibold text-slate-300 hover:text-white bg-obsidian-850 hover:bg-obsidian-800 border border-slate-700/60 rounded-lg transition"
                title="Tüm dalları genişlet"
              >
                Aç
              </button>
              <button
                type="button"
                onClick={handleCollapseAll}
                className="px-2 py-1 text-[11px] font-semibold text-slate-300 hover:text-white bg-obsidian-850 hover:bg-obsidian-800 border border-slate-700/60 rounded-lg transition"
                title="Tüm dalları daralt"
              >
                Kapat
              </button>

              <div className="h-4 w-px bg-slate-800 mx-1" />

              <button
                type="button"
                onClick={() => setZoom((z) => Math.min(z + 0.15, 2.0))}
                className="p-1.5 text-slate-400 hover:text-white bg-obsidian-850 hover:bg-obsidian-800 border border-slate-700/60 rounded-lg transition"
                title="Yakınlaştır"
              >
                <ZoomIn size={13} />
              </button>

              <span className="text-[10px] font-bold text-slate-400 min-w-[32px] text-center">
                {Math.round(zoom * 100)}%
              </span>

              <button
                type="button"
                onClick={() => setZoom((z) => Math.max(z - 0.15, 0.4))}
                className="p-1.5 text-slate-400 hover:text-white bg-obsidian-850 hover:bg-obsidian-800 border border-slate-700/60 rounded-lg transition"
                title="Uzaklaştır"
              >
                <ZoomOut size={13} />
              </button>

              <button
                type="button"
                onClick={handleResetView}
                className="p-1.5 text-slate-400 hover:text-white bg-obsidian-850 hover:bg-obsidian-800 border border-slate-700/60 rounded-lg transition"
                title="Görünümü Sıfırla"
              >
                <RotateCcw size={13} />
              </button>
            </div>
          )}
        </div>
      </div>

      {/* 1. ITEM-TO-ITEM NOTEBOOKLM TREE VIEW */}
      {viewMode === "tree" && (
        <div
          ref={containerRef}
          onMouseDown={handleMouseDown}
          onMouseMove={handleMouseMove}
          onMouseUp={handleMouseUp}
          onMouseLeave={handleMouseUp}
          className={cn(
            "relative w-full h-[520px] sm:h-[620px] overflow-hidden select-none touch-none bg-[radial-gradient(rgba(139,92,246,0.06)_1px,transparent_1px)] [background-size:24px_24px]",
            isDragging ? "cursor-grabbing" : "cursor-grab"
          )}
        >
          {/* Mobile touch gesture hint */}
          <div className="sm:hidden absolute top-2 right-2 bg-obsidian-900/90 border border-slate-800 px-2.5 py-1 rounded-lg text-[10px] text-slate-300 pointer-events-none z-10 backdrop-blur-md shadow-sm">
            👆 1 parmak: Kaydır · 2 parmak: Kıstır / Büyüt
          </div>

          {/* Desktop Ctrl + Wheel hint */}
          <div className="hidden sm:flex items-center gap-1.5 absolute bottom-3 right-3 bg-obsidian-900/90 border border-slate-800 px-2.5 py-1 rounded-lg text-[11px] text-slate-400 pointer-events-none z-10 backdrop-blur-md shadow-sm">
            <span className="font-semibold text-purple-400">Ctrl</span> + <span className="font-semibold text-purple-400">Tekerlek</span> ile yakınlaştır
          </div>

          {/* Transform Layer for Pan & Zoom */}
          <div
            style={{
              transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`,
              transformOrigin: "0 0",
              width: `${layout.totalWidth}px`,
              height: `${layout.totalHeight}px`,
              position: "relative",
              transition: isDragging ? "none" : "transform 0.1s ease-out",
            }}
          >
            {/* SVG Connecting Curves Between Items */}
            <svg
              className="absolute inset-0 pointer-events-none"
              width={layout.totalWidth}
              height={layout.totalHeight}
              style={{ overflow: "visible" }}
            >
              <defs>
                <linearGradient id="lineGrad" x1="0%" y1="0%" x2="100%" y2="0%">
                  <stop offset="0%" stopColor="#818cf8" stopOpacity="0.85" />
                  <stop offset="100%" stopColor="#a78bfa" stopOpacity="0.85" />
                </linearGradient>
              </defs>

              <AnimatePresence initial={false}>
                {layout.connections.map((conn) => (
                  <motion.path
                    key={conn.id}
                    d={conn.d}
                    fill="none"
                    stroke="url(#lineGrad)"
                    strokeWidth="2.5"
                    strokeLinecap="round"
                    initial={{ opacity: 0, pathLength: 0 }}
                    animate={{ opacity: 1, pathLength: 1 }}
                    exit={{ opacity: 0, pathLength: 0 }}
                    transition={{ duration: 0.28, ease: "easeInOut" }}
                  />
                ))}
              </AnimatePresence>
            </svg>

            {/* GUIDE ITEM NODES */}
            <AnimatePresence initial={false}>
              {layout.placedNodes.map((pn) => {
                return (
                <motion.div
                  key={pn.guide.publicId}
                  style={{
                    position: "absolute",
                    left: "0px",
                    top: "0px",
                    width: `${pn.width}px`,
                    height: `${pn.height}px`,
                  }}
                  initial={{ opacity: 0, x: pn.x, y: pn.y, scale: 0.96 }}
                  animate={{ opacity: 1, x: pn.x, y: pn.y, scale: 1 }}
                  exit={{ opacity: 0, scale: 0.96 }}
                  transition={{ duration: 0.28, ease: "easeInOut" }}
                  className="flex items-center"
                >
                  <div className="relative w-full h-full group">
                    <button
                      type="button"
                      onClick={() => onSelectGuide(pn.guide)}
                      className={cn(
                        "h-full text-left bg-[#182038] hover:bg-[#1f2a4a] active:bg-[#222e54] active:scale-[0.98] border border-slate-700/80 hover:border-purple-400 px-3.5 flex items-center justify-between shadow-lg hover:shadow-purple-900/20 transition cursor-pointer touch-manipulation",
                        pn.hasChildren ? "absolute left-0 w-4/5 rounded-l-2xl" : "w-full rounded-2xl"
                      )}
                    >
                      <div className="flex items-center gap-2.5 min-w-0 pr-3">
                        <div className="w-6 h-6 rounded-lg bg-purple-500/20 text-purple-300 border border-purple-500/30 flex items-center justify-center font-bold text-[11px] shrink-0">
                          {pn.stepNumber}
                        </div>

                        <div className="flex flex-col min-w-0">
                          <span className="text-xs font-semibold text-slate-100 group-hover:text-purple-200 truncate">
                            {pn.guide.title}
                          </span>
                          {!selectedModule && pn.guide.module && (
                            <span className="text-[10px] text-slate-400 truncate">
                              {pn.guide.module}
                            </span>
                          )}
                        </div>
                      </div>

                      {!pn.hasChildren && (
                        <div
                          className="w-5 h-5 rounded-full bg-[#141a2e] border border-slate-600 text-slate-400 group-hover:text-purple-300 group-hover:border-purple-400 flex items-center justify-center text-[10px] font-bold shrink-0 transition"
                          title="Rehberi Aç"
                        >
                          &gt;
                        </div>
                      )}
                    </button>

                    {/* The right fifth of a branch is a deliberately large toggle target. */}
                    {pn.hasChildren && (
                      <button
                        type="button"
                        onClick={(e) => {
                          e.stopPropagation();
                          toggleNode(pn.guide.publicId);
                        }}
                        aria-label={pn.isExpanded ? `${pn.guide.title} alt adımlarını daralt` : `${pn.guide.title} alt adımlarını göster`}
                        aria-expanded={pn.isExpanded}
                        className="absolute right-0 top-0 h-full w-1/5 min-w-[48px] rounded-r-2xl bg-[#141a2e] border border-l-0 border-slate-700/80 text-slate-300 hover:text-white hover:bg-[#1f2a4a] hover:border-purple-400 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-purple-400 focus-visible:ring-offset-2 focus-visible:ring-offset-[#141a2e] flex items-center justify-center text-xs font-bold shadow-lg cursor-pointer transition z-10 touch-manipulation active:scale-[0.98]"
                        title={pn.isExpanded ? "Alt adımları daralt" : "Alt adımları göster"}
                      >
                        <span
                          aria-hidden="true"
                          className={cn(
                            "w-6 h-6 rounded-full border border-slate-600 flex items-center justify-center transition-transform duration-300",
                            pn.isExpanded && "rotate-180"
                          )}
                        >
                          &gt;
                        </span>
                      </button>
                    )}
                  </div>
                </motion.div>
                );
              })}
            </AnimatePresence>
          </div>
        </div>
      )}

      {/* 2. MOBILE-FIRST ACCORDION LIST FLOW */}
      {viewMode === "list" && (
        <div className="p-4 space-y-3 max-h-[580px] overflow-y-auto hide-scrollbar">
          {treeRoots.length > 0 ? (
            treeRoots.map((root) => {
              return (
                <div
                  key={root.guide.publicId}
                  className="bg-obsidian-900/90 border border-slate-800 rounded-2xl p-3.5 space-y-2 shadow-lg"
                >
                  {/* Root Guide Item */}
                  <button
                    type="button"
                    onClick={() => onSelectGuide(root.guide)}
                    className="w-full text-left flex items-center justify-between p-3 rounded-xl bg-obsidian-850 hover:bg-obsidian-800/80 active:bg-[#202947] active:scale-[0.98] border border-slate-800 hover:border-purple-500/40 transition cursor-pointer group touch-manipulation"
                  >
                    <div className="flex items-center space-x-3 min-w-0">
                      <div className="w-6 h-6 rounded-lg bg-purple-500/20 text-purple-300 border border-purple-500/30 flex items-center justify-center font-bold text-xs shrink-0">
                        {root.stepNumber}
                      </div>
                      <div className="flex flex-col min-w-0">
                        <span className="text-xs font-semibold text-slate-100 group-hover:text-purple-300 truncate">
                          {root.guide.title}
                        </span>
                        {!selectedModule && root.guide.module && (
                          <span className="text-[10px] text-slate-400">
                            {root.guide.module}
                          </span>
                        )}
                      </div>
                    </div>

                    <div className="flex items-center space-x-2 shrink-0">
                      {root.guide.documents && root.guide.documents.length > 0 && (
                        <span className="text-[10px] text-slate-400 flex items-center gap-1">
                          <FileText size={11} className="text-purple-400" />
                          {root.guide.documents.length}
                        </span>
                      )}
                      <div className="text-xs font-semibold text-purple-400 group-hover:text-purple-300 flex items-center gap-0.5">
                        <span>İncele</span>
                        <ChevronRight size={13} />
                      </div>
                    </div>
                  </button>

                  {/* Connected Child Items (Follow-up Steps) */}
                  {root.children.length > 0 && (
                    <div className="pl-6 border-l-2 border-purple-500/20 space-y-2 mt-2">
                      {root.children.map((child) => (
                        <button
                          type="button"
                          key={child.guide.publicId}
                          onClick={() => onSelectGuide(child.guide)}
                          className="w-full text-left flex items-center justify-between p-2.5 rounded-xl bg-obsidian-850/70 hover:bg-obsidian-800/80 active:bg-[#202947] active:scale-[0.98] border border-slate-800/80 hover:border-purple-500/40 transition cursor-pointer group touch-manipulation"
                        >
                          <div className="flex items-center space-x-2.5 min-w-0">
                            <div className="w-5 h-5 rounded-md bg-purple-500/10 text-purple-400 border border-purple-500/20 flex items-center justify-center font-bold text-[10px] shrink-0">
                              {child.stepNumber}
                            </div>
                            <span className="text-xs font-medium text-slate-300 group-hover:text-purple-200 truncate">
                              {child.guide.title}
                            </span>
                          </div>

                          <ChevronRight
                            size={13}
                            className="text-slate-500 group-hover:text-purple-400 shrink-0"
                          />
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              );
            })
          ) : (
            <p className="text-xs text-slate-500 py-6 text-center">
              Bu filtreye uygun kılavuz akışı bulunamadı.
            </p>
          )}
        </div>
      )}
    </div>
  );
};
