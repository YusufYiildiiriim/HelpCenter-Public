"use client";

import React, { useEffect, useState, useCallback, useRef, useMemo } from "react";

// Services
import type { PublicGuide as CustomerGuide } from "@/services/public/PublicGuideService";
import { PublicGuideService as GuideService } from "@/services/public/PublicGuideService";
import type { PublicFaq as Faq } from "@/services/public/PublicFaqService";
import { PublicFaqService as SssService } from "@/services/public/PublicFaqService";
import type { PublicModule as Module } from "@/services/public/PublicModuleService";
import { PublicModuleService as ModuleService } from "@/services/public/PublicModuleService";
import type { PublicProject as Project } from "@/services/public/PublicProjectService";
import { PublicProjectService as ProjectService } from "@/services/public/PublicProjectService";

// Lib
import { usePageTitle } from "@/lib/usePageTitle";

// UI Components (4 Layers)
import { HomeNavbar } from "./components/home/HomeNavbar";
import { HeroSection } from "./components/home/HeroSection";
import { SupportTabs } from "./components/home/SupportTabs";
import { HomeFooter } from "./components/home/HomeFooter";
import { GuideReaderModal } from "./components/home/GuideReaderModal";
import { FaqDetailModal } from "./components/home/FaqDetailModal";

const PAGE_SIZE = 12;
const SEARCH_DEBOUNCE_MS = 350;

export default function HomePage() {
  usePageTitle("");

  const [activeTab, setActiveTab] = useState<"sss" | "guide" | "flow">("guide");
  const [projects, setProjects] = useState<Project[]>([]);
  const [selectedProjectId, setSelectedProjectId] = useState<number | null>(null);
  const [modules, setModules] = useState<Module[]>([]);
  const [selectedModule, setSelectedModule] = useState<string>("");

  // Server-paginated grid state.
  const [guides, setGuides] = useState<CustomerGuide[]>([]);
  const [guidesTotal, setGuidesTotal] = useState(0);
  const [guidesTotalPages, setGuidesTotalPages] = useState(1);
  const [guidesPage, setGuidesPage] = useState(1);
  const [guidesPageSize, setGuidesPageSize] = useState(PAGE_SIZE);

  const [faqs, setFaqs] = useState<Faq[]>([]);
  const [faqsTotal, setFaqsTotal] = useState(0);
  const [faqsTotalPages, setFaqsTotalPages] = useState(1);
  const [faqsPage, setFaqsPage] = useState(1);
  const [faqsPageSize, setFaqsPageSize] = useState(PAGE_SIZE);

  // Unfiltered project totals used by the footer.
  const [guidesGrandTotal, setGuidesGrandTotal] = useState(0);
  const [faqsGrandTotal, setFaqsGrandTotal] = useState(0);

  // Full chain used by the reader and mind map, independent from grid pagination.
  const [guideChain, setGuideChain] = useState<CustomerGuide[]>([]);

  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [sortBy, setSortBy] = useState<"newest" | "az" | "za">("newest");
  const [loading, setLoading] = useState(true);
  const [contentLoading, setContentLoading] = useState(false);
  const [selectedGuide, setSelectedGuide] = useState<CustomerGuide | null>(null);
  const [selectedFaq, setSelectedFaq] = useState<Faq | null>(null);

  const guidesSeqRef = useRef(0);
  const faqsSeqRef = useRef(0);
  const guideChainSeqRef = useRef(0);

  useEffect(() => {
    async function loadProjects() {
      try {
        const projectData = await ProjectService.getAll();
        setProjects(projectData);
        if (projectData.length > 0) {
          setSelectedProjectId(projectData[0].id);
        } else {
          setLoading(false);
        }
      } catch (err) {
        console.error("Projects load error:", err);
        setLoading(false);
      }
    }
    loadProjects();
  }, []);

  // 350ms debounce on search box — avoid sending requests on every keystroke.
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(searchTerm.trim()), SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(timer);
  }, [searchTerm]);

  // Load modules and select the first one when the project changes.
  useEffect(() => {
    if (!selectedProjectId) return;
    let isMounted = true;
    ModuleService.getAll(selectedProjectId)
      .then((moduleData) => {
        if (!isMounted) return;
        setModules(moduleData);
        setSelectedModule(moduleData.length > 0 ? moduleData[0].name : "");
      })
      .catch((err) => console.error("Modules load error:", err));
    return () => {
      isMounted = false;
    };
  }, [selectedProjectId]);

  // Load unfiltered project totals.
  useEffect(() => {
    if (!selectedProjectId) return;
    let isMounted = true;
    Promise.all([
      GuideService.getPaginated({ projectId: selectedProjectId, pageNumber: 1, pageSize: 1 }),
      SssService.getPaginated({ projectId: selectedProjectId, pageNumber: 1, pageSize: 1 }),
    ])
      .then(([guideRes, faqRes]) => {
        if (!isMounted) return;
        setGuidesGrandTotal(guideRes.totalCount);
        setFaqsGrandTotal(faqRes.totalCount);
      })
      .catch((err) => console.error("Grand total load error:", err));
    return () => {
      isMounted = false;
    };
  }, [selectedProjectId]);

  const fetchGuidesPage = useCallback(
    async (page: number, size: number) => {
      if (!selectedProjectId) return;
      const seq = ++guidesSeqRef.current;
      try {
        const res = await GuideService.getPaginated({
          projectId: selectedProjectId,
          module: selectedModule || undefined,
          search: debouncedSearch || undefined,
          pageNumber: page,
          pageSize: size,
        });
        if (seq !== guidesSeqRef.current) return;
        setGuides(res.items);
        setGuidesTotal(res.totalCount);
        setGuidesTotalPages(res.totalPages || 1);
        setGuidesPage(res.currentPage);
      } catch (err) {
        console.error("Guides load error:", err);
      }
    },
    [selectedProjectId, selectedModule, debouncedSearch]
  );

  const fetchFaqsPage = useCallback(
    async (page: number, size: number) => {
      if (!selectedProjectId) return;
      const seq = ++faqsSeqRef.current;
      const selectedModuleId = modules.find((m) => m.name === selectedModule)?.id;
      try {
        const res = await SssService.getPaginated({
          projectId: selectedProjectId,
          moduleId: selectedModuleId,
          search: debouncedSearch || undefined,
          pageNumber: page,
          pageSize: size,
        });
        if (seq !== faqsSeqRef.current) return;
        setFaqs(res.items);
        setFaqsTotal(res.totalCount);
        setFaqsTotalPages(res.totalPages || 1);
        setFaqsPage(res.currentPage);
      } catch (err) {
        console.error("Faqs load error:", err);
      }
    },
    [selectedProjectId, selectedModule, debouncedSearch, modules]
  );

  // Filters reset both grids to their first page.
  useEffect(() => {
    if (!selectedProjectId) return;
    // eslint-disable-next-line react-hooks/set-state-in-effect -- intentional fetch triggering on project/module/search change
    setContentLoading(true);
    Promise.allSettled([fetchGuidesPage(1, guidesPageSize), fetchFaqsPage(1, faqsPageSize)]).finally(() => {
      setContentLoading(false);
      setLoading(false);
    });
    // Page-size changes fetch directly through their dedicated handlers.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedProjectId, selectedModule, debouncedSearch, fetchGuidesPage, fetchFaqsPage]);

  // Reload the full chain when project or module changes.
  useEffect(() => {
    if (!selectedProjectId) return;
    const seq = ++guideChainSeqRef.current;
    GuideService.getAllForChain({ projectId: selectedProjectId, module: selectedModule || undefined })
      .then((all) => {
        if (seq !== guideChainSeqRef.current) return;
        setGuideChain(all);
      })
      .catch((err) => console.error("Guide chain load error:", err));
  }, [selectedProjectId, selectedModule]);

  useEffect(() => {
    if (selectedGuide || selectedFaq) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "auto";
    }
    return () => {
      document.body.style.overflow = "auto";
    };
  }, [selectedGuide, selectedFaq]);

  const handleGuidesPageChange = (page: number) => {
    setContentLoading(true);
    fetchGuidesPage(page, guidesPageSize).finally(() => setContentLoading(false));
  };

  const handleGuidesPageSizeChange = (size: number) => {
    setGuidesPageSize(size);
    setContentLoading(true);
    fetchGuidesPage(1, size).finally(() => setContentLoading(false));
  };

  const handleFaqsPageChange = (page: number) => {
    setContentLoading(true);
    fetchFaqsPage(page, faqsPageSize).finally(() => setContentLoading(false));
  };

  const handleFaqsPageSizeChange = (size: number) => {
    setFaqsPageSize(size);
    setContentLoading(true);
    fetchFaqsPage(1, size).finally(() => setContentLoading(false));
  };

  const handleNextStep = () => {
    if (!selectedGuide) return;
    const next = guideChain.find((g) => g.previousGuidePublicId === selectedGuide.publicId);
    if (next) setSelectedGuide(next);
  };

  const handlePrevStep = () => {
    if (!selectedGuide) return;
    const prev = guideChain.find((g) => g.publicId === selectedGuide.previousGuidePublicId);
    if (prev) setSelectedGuide(prev);
  };

  const getSequencedGuides = (moduleName?: string) => {
    const targetModule = moduleName || selectedModule;
    const moduleGuides = !targetModule ? guideChain : guideChain.filter((g) => g.module === targetModule);

    const sequenced: CustomerGuide[] = [];

    let current = moduleGuides.find(
      (g) => !g.previousGuidePublicId || !moduleGuides.find((prev) => prev.publicId === g.previousGuidePublicId)
    );

    while (current && sequenced.length < moduleGuides.length) {
      sequenced.push(current);
      const nextPublicId = current.publicId;
      current = moduleGuides.find((g) => g.previousGuidePublicId === nextPublicId);
    }

    moduleGuides.forEach((g) => {
      if (!sequenced.find((s) => s.publicId === g.publicId)) sequenced.push(g);
    });

    return sequenced;
  };

  const selectedProject = useMemo(() => projects.find((p) => p.id === selectedProjectId), [projects, selectedProjectId]);

  return (
    <div className="dark bg-obsidian-950 text-slate-200 font-sans min-h-screen antialiased selection:bg-purple-600/30 selection:text-purple-200 aurora-gradient flex flex-col justify-between relative overflow-x-hidden">
      <div aria-hidden="true" className="fixed inset-0 pointer-events-none z-0 overflow-hidden">
        <div className="absolute -top-40 left-1/2 -translate-x-1/2 w-[720px] h-[340px] bg-purple-600/10 blur-[130px] rounded-full" />
        <div className="absolute top-[40%] right-[-10%] w-[450px] h-[350px] bg-indigo-600/5 blur-[120px] rounded-full" />
        <div className="absolute bottom-[-10%] left-[-5%] w-[400px] h-[300px] bg-fuchsia-600/5 blur-[110px] rounded-full" />
      </div>

      <GuideReaderModal
        selectedGuide={selectedGuide}
        setSelectedGuide={setSelectedGuide}
        guides={guideChain}
        handleNextStep={handleNextStep}
        handlePrevStep={handlePrevStep}
        getSequencedGuides={getSequencedGuides}
      />

      <FaqDetailModal selectedFaq={selectedFaq} setSelectedFaq={setSelectedFaq} />

      <div className="relative z-10 max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-6 space-y-6 flex-1 flex flex-col justify-between">
        <HomeNavbar
          projects={projects}
          selectedProjectId={selectedProjectId}
          onProjectChange={setSelectedProjectId}
        />

        <HeroSection
          searchTerm={searchTerm}
          setSearchTerm={setSearchTerm}
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          guidesCount={guidesTotal}
          faqsCount={faqsTotal}
          flowsCount={guideChain.length}
          modules={modules}
          selectedModule={selectedModule}
          setSelectedModule={setSelectedModule}
          sortBy={sortBy}
          setSortBy={setSortBy}
          placeholder={`${selectedProject?.name || "Help Center"} içinde arama yapın, kılavuz veya çözüm sorgulayın...`}
        />

        <SupportTabs
          activeTab={activeTab}
          setActiveTab={setActiveTab}
          loading={loading}
          contentLoading={contentLoading}
          filteredFaqs={faqs}
          modules={modules}
          selectedModule={selectedModule}
          setSelectedModule={setSelectedModule}
          filteredGuides={guides}
          setSelectedGuide={setSelectedGuide}
          onSelectFaq={setSelectedFaq}
          allGuides={guideChain}
          sortBy={sortBy}
          projectName={selectedProject?.name || "Yardım Merkezi"}
          guidesPagination={{ currentPage: guidesPage, totalPages: guidesTotalPages, totalCount: guidesTotal, pageSize: guidesPageSize }}
          onGuidesPageChange={handleGuidesPageChange}
          onGuidesPageSizeChange={handleGuidesPageSizeChange}
          faqsPagination={{ currentPage: faqsPage, totalPages: faqsTotalPages, totalCount: faqsTotal, pageSize: faqsPageSize }}
          onFaqsPageChange={handleFaqsPageChange}
          onFaqsPageSizeChange={handleFaqsPageSizeChange}
        />

        <HomeFooter
          modulesCount={modules.length}
          guidesCount={guidesGrandTotal}
          faqsCount={faqsGrandTotal}
        />
      </div>
    </div>
  );
}
