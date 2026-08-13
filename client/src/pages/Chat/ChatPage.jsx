import React, { useState, useEffect, useRef, useCallback } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import * as signalR from '@microsoft/signalr';
import axiosInstance from '../../services/axiosInstance';
import {
  Box,
  Paper,
  Typography,
  IconButton,
  TextField,
  InputAdornment,
  List,
  ListItemButton,
  ListItemAvatar,
  ListItemText,
  Avatar,
  Badge,
  Divider,
  CircularProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  Tabs,
  Tab,
  MenuItem,
  Button,
  Tooltip,
  useMediaQuery,
} from '@mui/material';
import { useTheme } from '@mui/material/styles';
import SendIcon from '@mui/icons-material/Send';
import ChatBubbleIcon from '@mui/icons-material/ChatBubble';
import GroupIcon from '@mui/icons-material/Group';
import SearchIcon from '@mui/icons-material/Search';
import AddIcon from '@mui/icons-material/Add';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import {
  fetchConversations,
  fetchMessages,
  sendMessage,
  markChatRead,
  fetchAvailableUsers,
  setActiveConversation,
  receiveMessage,
} from '../../store/slices/chatSlice';

const ADMIN_ROLE_NAMES = ['SuperAdmin', 'Admin', 'SchoolAdmin', 'Principal', 'VicePrincipal'];

const initials = (name) =>
  (name || '?')
    .split(' ')
    .filter(Boolean)
    .map((word) => word[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();

const formatTime = (iso) => {
  if (!iso) return '';
  return new Date(iso).toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  });
};

const formatDayLabel = (iso) => {
  if (!iso) return '';
  const date = new Date(iso);
  const today = new Date();
  if (date.toDateString() === today.toDateString()) return 'Today';
  const yesterday = new Date(today);
  yesterday.setDate(today.getDate() - 1);
  if (date.toDateString() === yesterday.toDateString()) return 'Yesterday';
  return date.toLocaleDateString([], {
    month: 'short',
    day: 'numeric',
    year: date.getFullYear() === today.getFullYear() ? undefined : 'numeric',
  });
};

export default function ChatPage() {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const dispatch = useDispatch();

  const user = useSelector((state) => state.auth.user);
  const { conversations, messages, availableUsers, loading, sending, error } =
    useSelector((state) => state.chat);
  const activeConversation = useSelector(
    (state) => state.chat.activeConversation
  );

  const currentUserId = user?.id;
  const userRole = user?.roles?.[0] || user?.role || 'Admin';
  const isStudent = userRole === 'Student';
  const isAdmin = ADMIN_ROLE_NAMES.includes(userRole);

  const [connection, setConnection] = useState(null);
  const [inputText, setInputText] = useState('');
  const [conversationSearch, setConversationSearch] = useState('');
  const [newChatOpen, setNewChatOpen] = useState(false);
  const [userTab, setUserTab] = useState(0);
  const [userSearch, setUserSearch] = useState('');
  const [classes, setClasses] = useState([]);
  const [sections, setSections] = useState([]);
  const [selectedClassId, setSelectedClassId] = useState('');
  const [selectedSectionId, setSelectedSectionId] = useState('');
  const messagesEndRef = useRef(null);
  const connectionRef = useRef(null);

  const joinSection = useCallback((sectionId) => {
    if (connectionRef.current && sectionId) {
      connectionRef.current.invoke('JoinSection', sectionId).catch(() => {});
    }
  }, []);

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token || !user) return;

    const conn = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/chat', { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build();

    conn.on('ReceiveMessage', (message) => {
      dispatch(receiveMessage({ message, currentUserId: user.id }));
      dispatch(fetchConversations());
    });

    conn.start().catch(() => {});

    connectionRef.current = conn;
    setConnection(conn);

    return () => {
      conn.stop().catch(() => {});
      connectionRef.current = null;
    };
  }, [dispatch, user]);

  useEffect(() => {
    dispatch(fetchConversations());
  }, [dispatch]);

  useEffect(() => {
    if (!conversations || conversations.length === 0 || !isStudent) return;
    const classConv = conversations.find((c) => c.type === 'Class');
    if (classConv?.sectionId) joinSection(classConv.sectionId);
  }, [conversations, isStudent, joinSection]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  useEffect(() => {
    if (newChatOpen) {
      setUserSearch('');
      setUserTab(0);
      fetchUsersForTab(0, '');
      if (!isStudent) {
        axiosInstance.get('/schools').then(async (res) => {
          const schools = res.data?.data?.items || [];
          if (schools.length > 0) {
            const classRes = await axiosInstance.get(
              `/schools/${schools[0].id}/classes`
            );
            const classData = classRes.data?.data;
            setClasses(Array.isArray(classData) ? classData : []);
          }
        });
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [newChatOpen]);

  const fetchUsersForTab = useCallback(
    async (tabIndex, search) => {
      let role = 'Student';
      if (tabIndex === 1) role = 'Teacher';
      else if (tabIndex === 2) role = 'Staff';
      dispatch(fetchAvailableUsers({ role, search }));
    },
    [dispatch]
  );

  useEffect(() => {
    if (!newChatOpen) return;
    const timer = setTimeout(() => {
      fetchUsersForTab(userTab, userSearch);
    }, 250);
    return () => clearTimeout(timer);
  }, [userTab, userSearch, newChatOpen, fetchUsersForTab]);

  const openConversation = useCallback(
    async (conversation) => {
      dispatch(setActiveConversation(conversation));
      dispatch(
        fetchMessages({
          peerUserId: conversation.peerUserId,
          sectionId: conversation.sectionId,
        })
      );
      if (conversation.type === 'Class') {
        joinSection(conversation.sectionId);
      } else if (conversation.peerUserId) {
        dispatch(markChatRead(conversation.peerUserId));
      }
    },
    [dispatch, joinSection]
  );

  const handleSend = async () => {
    const text = inputText.trim();
    if (!text || !activeConversation || sending) return;

    const payload =
      activeConversation.type === 'Class'
        ? {
            message: text,
            sectionId: activeConversation.sectionId,
            messageType: 'Class',
          }
        : {
            message: text,
            receiverId: activeConversation.peerUserId,
            messageType: 'Direct',
          };

    setInputText('');
    const result = await dispatch(sendMessage(payload));
    if (result.error) {
      setInputText(text);
    }
  };

  const openDirectChat = (contact) => {
    setNewChatOpen(false);
    const conversation = {
      id: `direct:${contact.id}`,
      type: 'Direct',
      title: contact.fullName,
      subtitle: contact.role,
      peerUserId: contact.id,
      unreadCount: 0,
    };
    openConversation(conversation);
  };

  const openClassChat = () => {
    if (isStudent) {
      const classConv = conversations.find((c) => c.type === 'Class');
      if (classConv) {
        setNewChatOpen(false);
        openConversation(classConv);
      }
      return;
    }
    if (!selectedSectionId) return;
    const section = sections.find((s) => s.id === selectedSectionId);
    const classRoom = classes.find((c) => c.id === selectedClassId);
    const conversation = {
      id: `class:${selectedSectionId}`,
      type: 'Class',
      title: `${classRoom?.name || 'Class'} - Section ${section?.name || ''}`.trim(),
      subtitle: 'Class Chat',
      sectionId: selectedSectionId,
      unreadCount: 0,
    };
    setNewChatOpen(false);
    openConversation(conversation);
  };

  const handleSectionChange = async (classId) => {
    setSelectedClassId(classId);
    setSelectedSectionId('');
    setSections([]);
    const res = await axiosInstance.get(`/schools/classes/${classId}/sections`);
    setSections(res.data?.data || []);
  };

  const filteredConversations = (conversations || []).filter((conv) => {
    if (!conversationSearch.trim()) return true;
    return conv.title.toLowerCase().includes(conversationSearch.toLowerCase());
  });

  const roleLabel = (subtitle) => {
    if (!subtitle) return '';
    if (subtitle === 'ClassChat' || subtitle === 'Class Chat') return 'Class Chat';
    return subtitle;
  };

  const shouldShowSender = activeConversation?.type === 'Class';

  return (
    <Box sx={{ display: 'flex', gap: 2, height: 'calc(100vh - 120px)' }}>
      <Paper
        elevation={0}
        sx={{
          width: isMobile ? (activeConversation ? 0 : '100%') : 340,
          flexShrink: 0,
          display: 'flex',
          flexDirection: 'column',
          overflow: 'hidden',
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 2,
        }}
      >
        <Box sx={{ p: 2, borderBottom: '1px solid', borderColor: 'divider' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1.5 }}>
            <Typography variant="h6" fontWeight={700}>Chats</Typography>
            <Tooltip title="Start new chat">
              <IconButton size="small" onClick={() => setNewChatOpen(true)} color="primary">
                <AddIcon />
              </IconButton>
            </Tooltip>
          </Box>
          <TextField
            size="small"
            fullWidth
            placeholder="Search conversations..."
            value={conversationSearch}
            onChange={(e) => setConversationSearch(e.target.value)}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>
              ),
            }}
          />
        </Box>

        <Box sx={{ flex: 1, overflowY: 'auto' }}>
          {loading && conversations.length === 0 ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
              <CircularProgress size={28} />
            </Box>
          ) : filteredConversations.length === 0 ? (
            <Box sx={{ p: 4, textAlign: 'center' }}>
              <ChatBubbleIcon color="disabled" sx={{ fontSize: 40, mb: 1 }} />
              <Typography variant="body2" color="text.secondary">
                No conversations yet.
              </Typography>
              <Button size="small" sx={{ mt: 1 }} onClick={() => setNewChatOpen(true)}>
                Start a chat
              </Button>
            </Box>
          ) : (
            <List disablePadding>
              {filteredConversations.map((conv) => {
                const active = activeConversation?.id === conv.id;
                return (
                  <ListItemButton
                    key={conv.id}
                    selected={active}
                    onClick={() => openConversation(conv)}
                    sx={{
                      py: 1.5,
                      borderBottom: '1px solid',
                      borderColor: 'divider',
                      '&.Mui-selected': {
                        backgroundColor: 'primary.light',
                      },
                    }}
                  >
                    <ListItemAvatar>
                      <Avatar sx={{ bgcolor: conv.type === 'Class' ? 'success.main' : 'primary.main' }}>
                        {conv.type === 'Class' ? <GroupIcon /> : initials(conv.title)}
                      </Avatar>
                    </ListItemAvatar>
                    <ListItemText
                      primary={
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2" fontWeight={600} noWrap>
                            {conv.title}
                          </Typography>
                          <Typography variant="caption" color="text.disabled" sx={{ flexShrink: 0 }}>
                            {formatTime(conv.lastMessageAt)}
                          </Typography>
                        </Box>
                      }
                      secondary={
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 1 }}>
                          <Typography variant="caption" color="text.secondary" noWrap sx={{ flex: 1 }}>
                            {conv.lastMessage || 'No messages yet'}
                          </Typography>
                          {conv.unreadCount > 0 && (
                            <Badge
                              badgeContent={conv.unreadCount}
                              color="error"
                              sx={{ flexShrink: 0, '& .MuiBadge-badge': { fontSize: 10 } }}
                            />
                          )}
                        </Box>
                      }
                    />
                  </ListItemButton>
                );
              })}
            </List>
          )}
        </Box>
      </Paper>

      <Paper
        elevation={0}
        sx={{
          flex: 1,
          display: activeConversation || !isMobile ? 'flex' : 'none',
          flexDirection: 'column',
          overflow: 'hidden',
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 2,
          bgcolor: 'background.paper',
        }}
      >
        {!activeConversation ? (
          <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100%', p: 4, textAlign: 'center' }}>
            <ChatBubbleIcon color="disabled" sx={{ fontSize: 56, mb: 2 }} />
            <Typography variant="h6" color="text.secondary">
              Select a conversation
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Chat with teachers, students, or your whole class.
            </Typography>
          </Box>
        ) : (
          <>
            <Box
              sx={{
                px: 2,
                py: 1.5,
                display: 'flex',
                alignItems: 'center',
                gap: 1.5,
                borderBottom: '1px solid',
                borderColor: 'divider',
              }}
            >
              {isMobile && (
                <IconButton size="small" onClick={() => dispatch(setActiveConversation(null))}>
                  <ArrowBackIcon fontSize="small" />
                </IconButton>
              )}
              <Avatar
                sx={{
                  width: 40,
                  height: 40,
                  bgcolor: activeConversation.type === 'Class' ? 'success.main' : 'primary.main',
                }}
              >
                {activeConversation.type === 'Class' ? <GroupIcon /> : initials(activeConversation.title)}
              </Avatar>
              <Box sx={{ minWidth: 0 }}>
                <Typography variant="subtitle1" fontWeight={700} noWrap>
                  {activeConversation.title}
                </Typography>
                <Typography variant="caption" color="text.secondary" noWrap>
                  {roleLabel(activeConversation.subtitle) || (activeConversation.type === 'Class' ? 'Class Chat' : '')}
                </Typography>
              </Box>
            </Box>

            <Box
              sx={{
                flex: 1,
                overflowY: 'auto',
                px: 2,
                py: 2,
                bgcolor: 'background.default',
                display: 'flex',
                flexDirection: 'column',
              }}
            >
              {messages.map((message, index) => {
                const isMine = message.senderId === currentUserId;
                const prev = messages[index - 1];
                const showDayDivider = !prev || formatDayLabel(prev.createdAt) !== formatDayLabel(message.createdAt);
                return (
                  <React.Fragment key={message.id}>
                    {showDayDivider && (
                      <Box sx={{ textAlign: 'center', my: 1 }}>
                        <Typography
                          variant="caption"
                          sx={{ bgcolor: 'action.hover', px: 1.5, py: 0.5, borderRadius: 10, color: 'text.secondary' }}
                        >
                          {formatDayLabel(message.createdAt)}
                        </Typography>
                      </Box>
                    )}
                    <Box
                      sx={{
                        display: 'flex',
                        justifyContent: isMine ? 'flex-end' : 'flex-start',
                        mb: 0.75,
                      }}
                    >
                      <Box
                        sx={{
                          maxWidth: '72%',
                          bgcolor: isMine ? 'primary.main' : 'background.paper',
                          color: isMine ? 'primary.contrastText' : 'text.primary',
                          px: 1.5,
                          py: 1,
                          borderRadius: 2,
                          border: isMine ? 'none' : '1px solid',
                          borderColor: 'divider',
                          boxShadow: '0 1px 2px rgba(0,0,0,0.05)',
                        }}
                      >
                        {shouldShowSender && !isMine && (
                          <Typography variant="caption" fontWeight={700} sx={{ display: 'block', color: 'success.main' }}>
                            {message.senderName}
                          </Typography>
                        )}
                        <Typography variant="body2" sx={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
                          {message.message}
                        </Typography>
                        <Typography
                          variant="caption"
                          sx={{ display: 'block', textAlign: 'right', mt: 0.25, opacity: 0.75 }}
                        >
                          {formatTime(message.createdAt)}
                        </Typography>
                      </Box>
                    </Box>
                  </React.Fragment>
                );
              })}
              <div ref={messagesEndRef} />
            </Box>

            <Box sx={{ p: 2, borderTop: '1px solid', borderColor: 'divider', display: 'flex', gap: 1 }}>
              <TextField
                fullWidth
                size="small"
                placeholder="Type a message..."
                value={inputText}
                onChange={(e) => setInputText(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    handleSend();
                  }
                }}
                multiline
                maxRows={4}
                InputProps={{
                  style: { borderRadius: 12 },
                }}
              />
              <IconButton
                color="primary"
                onClick={handleSend}
                disabled={!inputText.trim() || sending}
                sx={{
                  alignSelf: 'flex-end',
                  bgcolor: 'primary.main',
                  color: 'primary.contrastText',
                  '&:hover': { bgcolor: 'primary.dark' },
                  '&.Mui-disabled': { bgcolor: 'action.disabledBackground' },
                }}
              >
                <SendIcon fontSize="small" />
              </IconButton>
            </Box>
          </>
        )}
      </Paper>

      <Dialog
        open={newChatOpen}
        onClose={() => setNewChatOpen(false)}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle sx={{ pb: 1 }}>Start a new chat</DialogTitle>
        <DialogContent sx={{ pb: 2 }}>
          <Tabs
            value={userTab}
            onChange={(_, value) => {
              setUserTab(value);
              setSelectedClassId('');
              setSelectedSectionId('');
            }}
            variant="fullWidth"
          >
            <Tab label="Students" />
            <Tab label="Teachers" />
            <Tab label="Staff" />
            {!isStudent && <Tab label="Class Chat" />}
          </Tabs>

          {userTab === 3 ? (
            <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                select
                label="Class"
                value={selectedClassId}
                onChange={(e) => handleSectionChange(e.target.value)}
                fullWidth
              >
                {classes.map((c) => (
                  <MenuItem key={c.id} value={c.id}>{c.name}</MenuItem>
                ))}
              </TextField>
              <TextField
                select
                label="Section"
                value={selectedSectionId}
                onChange={(e) => setSelectedSectionId(e.target.value)}
                fullWidth
                disabled={!selectedClassId}
              >
                {sections.map((s) => (
                  <MenuItem key={s.id} value={s.id}>Section {s.name}</MenuItem>
                ))}
              </TextField>
              <Button
                variant="contained"
                disabled={!selectedSectionId}
                onClick={openClassChat}
                startIcon={<GroupIcon />}
              >
                Open class chat
              </Button>
            </Box>
          ) : (
            <>
              <TextField
                size="small"
                fullWidth
                sx={{ mt: 1.5 }}
                placeholder="Search by name or email..."
                value={userSearch}
                onChange={(e) => setUserSearch(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start"><SearchIcon fontSize="small" /></InputAdornment>
                  ),
                }}
              />
              <Box sx={{ maxHeight: 360, overflowY: 'auto', mt: 1 }}>
                {availableUsers.length === 0 ? (
                  <Box sx={{ p: 3, textAlign: 'center' }}>
                    <Typography variant="body2" color="text.secondary">
                      No users found.
                    </Typography>
                  </Box>
                ) : (
                  <List disablePadding>
                    {availableUsers.map((contact) => (
                      <ListItemButton key={contact.id} onClick={() => openDirectChat(contact)} sx={{ borderRadius: 1 }}>
                        <ListItemAvatar>
                          <Avatar>{initials(contact.fullName)}</Avatar>
                        </ListItemAvatar>
                        <ListItemText
                          primary={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                              <Typography variant="body2" fontWeight={600}>
                                {contact.fullName}
                              </Typography>
                              {contact.sectionName && (
                                <Typography variant="caption" color="success.main" fontWeight={600}>
                                  {contact.sectionName}
                                </Typography>
                              )}
                            </Box>
                          }
                          secondary={
                            <Typography variant="caption" color="text.secondary">
                              {contact.role}{contact.role ? ' · ' : ''}{contact.email}
                            </Typography>
                          }
                        />
                      </ListItemButton>
                    ))}
                  </List>
                )}
              </Box>
            </>
          )}
        </DialogContent>
      </Dialog>

      {error && (
        <Box sx={{ position: 'fixed', bottom: 16, left: '50%', transform: 'translateX(-50%)', zIndex: 1300 }}>
          <Typography variant="body2" sx={{ bgcolor: 'error.main', color: 'white', px: 2, py: 1, borderRadius: 2 }}>
            {error}
          </Typography>
        </Box>
      )}
    </Box>
  );
}
